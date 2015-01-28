using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ActionStreetMap.Infrastructure.IO.Compression
{
    /// <summary> Refactored implementation of some code found on Internet. </summary>
    internal class InflateZipStream : InflateMethod
    {
        private List<FileHeader> _zipDir;
        private Int64 _zipDirPosition;

        private Stream _inputStream;
        private BinaryReader _streamReader;
        private UInt32 _readRemain;

        private MemoryStream _outputStream;
        private BinaryWriter _streamWriter;
        private UInt32 _writeCRC32;

        // active ZIP file info
        private FileHeader _fileHeaderInfo;

        public IEnumerable<KeyValuePair<string,byte[]>> Unzip(Stream inputStream)
        {
            // open the zip archive
            OpenZipFile(inputStream);

            // decompress all files
            foreach (FileHeader fh in _zipDir)
                yield return DecompressZipFile(fh);
        }

        private void OpenZipFile(Stream inputStream)
        {
            // trap errors
            try
            {
                _inputStream = inputStream;
                // convert stream to binary reader
                _streamReader = new BinaryReader(_inputStream, Encoding.UTF8);
                // read zip directory
                ReadZipFileDirectory();
            }
            catch
            {
                CloseZipFile();
                throw;
            }
        }

        private void CloseZipFile()
        {
            if (_streamReader != null)
            {
                _streamReader.Close();
                _streamReader = null;
            }
            if (_zipDir != null)
            {
                _zipDir.Clear();
                _zipDir = null;
            }
        }

        private KeyValuePair<string,byte[]> DecompressZipFile(FileHeader fh)
        {
            try
            {
                // save file header
                _fileHeaderInfo = fh;

                // read file header for this file and compare it to the directory information
                var fileName = ReadFileHeader(_fileHeaderInfo.FilePos);

                // compressed length
                _readRemain = _fileHeaderInfo.CompSize;

                // create destination file
                _outputStream = new MemoryStream();

                // convert stream to binary writer
                _streamWriter = new BinaryWriter(_outputStream, Encoding.UTF8);

                // reset crc32 checksum
                _writeCRC32 = 0;

                // switch based on compression method
                switch (_fileHeaderInfo.CompMethod)
                {
                        // no compression
                    case 0:
                        NoCompression();
                        break;

                        // deflate compress method
                    case 8:
                        // decompress file
                        Decompress();
                        break;

                        // not supported
                    default:
                        throw new ApplicationException("Unsupported compression method");
                }

                // Zip file checksum is CRC32
                if (_fileHeaderInfo.FileCRC32 != _writeCRC32)
                    throw new ApplicationException("ZIP file CRC test failed");

                var bytes = _outputStream.GetBuffer()
                    .Take((int) _outputStream.Length).ToArray();
                _streamWriter.Close();

                return new KeyValuePair<string, byte[]>(fileName, bytes);
            }
            catch
            {
                // close the write file if it is open
                if (_streamWriter != null)
                {
                    _streamWriter.Close();
                    _streamWriter = null;
                }
                throw;
            }
        }

        private void ReadZipFileDirectory()
        {
            //	End of central directory record:
            //	Pos		Len
            //	0		4		End of central directory signature = 0x06054b50
            //	4		2		Number of this disk
            //	6		2		Disk where central directory starts
            //	8		2		Number of central directory records on this disk
            //	10		2		Total number of central directory records
            //	12		4		Size of central directory (bytes)
            //	16		4		Offset of start of central directory, relative to start of archive
            //	20		2		ZIP file comment length (n)
            //	22		n		ZIP file comment
            //
            //	Central directory file header
            //
            //	Pos		Len
            //	0		4		Central directory file header signature = 0x02014b50
            //	4		2		Version made by
            //	6		2		Version needed to extract (minimum)
            //	8		2		General purpose bit flag
            //	10		2		Compression method
            //	12		2		File last modification time
            //	14		2		File last modification date
            //	16		4		CRC-32
            //	20		4		Compressed size
            //	24		4		Uncompressed size
            //	28		2		File name length (n)
            //	30		2		Extra field length (m)
            //	32		2		File comment length (k)
            //	34		2		Disk number where file starts
            //	36		2		Internal file attributes
            //	38		4		External file attributes
            //	42		4		Offset of local file header
            //	46		n		File name
            //	46+n	m		Extra field
            //	46+n+m	k		File comment

            // file length
            Int64 fileLen = _inputStream.Length;
            if (fileLen < 98) throw new ApplicationException("ZIP file is too short");

            // read last 512 byte block at the end of the file
            if (fileLen > 512) _inputStream.Position = fileLen - 512;
            Byte[] dirSig = _streamReader.ReadBytes(512);

            // look for signature
            Int32 ptr;
            for (ptr = dirSig.Length - 20; ptr >= 0 && BitConverter.ToInt32(dirSig, ptr) != 0x06054b50; ptr--) ;
            if (ptr < 0) throw new ApplicationException("Invalid ZIP file (No central directory)");
            ptr += 4;

            // number of this disk should be zero
            Int16 diskNo = BitConverter.ToInt16(dirSig, ptr);
            ptr += 2;
            if (diskNo != 0) throw new ApplicationException("No support for multi-disk ZIP file");

            // disk where central directory starts should be zero
            Int16 dirDiskNo = BitConverter.ToInt16(dirSig, ptr);
            ptr += 2;
            if (dirDiskNo != 0) throw new ApplicationException("No support for multi-disk ZIP file");

            // number of central directory records on this disk
            Int16 dirEntries1 = BitConverter.ToInt16(dirSig, ptr);
            ptr += 2;

            // Total number of central directory records
            Int16 dirEntries = BitConverter.ToInt16(dirSig, ptr);
            ptr += 2;
            if (dirEntries == 0 || dirEntries != dirEntries1)
                throw new ApplicationException("Central directory is empty or in error");

            // Size of central directory (bytes)
            Int32 dirSize = BitConverter.ToInt32(dirSig, ptr);
            ptr += 4;
            if (dirSize == 0) throw new ApplicationException("Central directory empty");

            // Offset of start of central directory, relative to start of archive
            _zipDirPosition = BitConverter.ToInt32(dirSig, ptr);
            if (_zipDirPosition == 0) throw new ApplicationException("Central directory empty");

            // create result array
            _zipDir = new List<FileHeader>(dirEntries);

            // position file to central directory
            _inputStream.Position = _zipDirPosition;

            // read central directory
            while (dirEntries-- > 0)
            {
                // file header
                FileHeader fh = new FileHeader();

                // Central directory file header signature = 0x02014b50
                Int32 fileDirSig = _streamReader.ReadInt32();
                if (fileDirSig != 0x02014b50) throw new ApplicationException("File directory signature error");

                // Version made by (ignored)
                Int32 versionMadeBy = _streamReader.ReadInt16();

                // Low byte is version needed to extract (the low byte should be 20 for version 2.0).
                // High byte is a computer system code that define the extrenal file attribute.
                // If high byte is zero it is DOS compatible.
                fh.Version = _streamReader.ReadUInt16();
                fh.BitFlags = _streamReader.ReadUInt16();
                fh.CompMethod = _streamReader.ReadUInt16();
                fh.FileTime = _streamReader.ReadUInt16();
                fh.FileDate = _streamReader.ReadUInt16();
                fh.FileCRC32 = _streamReader.ReadUInt32();
                fh.CompSize = _streamReader.ReadUInt32();
                fh.FileSize = _streamReader.ReadUInt32();
                Int32 fileNameLen = _streamReader.ReadInt16();
                Int32 extraFieldLen = _streamReader.ReadInt16();
                Int32 commentLen = _streamReader.ReadInt16();
                Int32 fileDiskNo = _streamReader.ReadInt16();
                if (fileDiskNo != 0)
                    throw new ApplicationException("No support for multi-disk ZIP file");

                // internal file attributes (ignored)
                Int32 fileIntAttr = _streamReader.ReadInt16();

                // external file attributes
                fh.FileAttr = (FileAttributes) _streamReader.ReadUInt32();

                // if file system is not FAT or equivalent, erase the attributes
                if ((fh.Version & 0xff00) != 0) fh.FileAttr = 0;

                // file position
                fh.FilePos = _streamReader.ReadUInt32();

                // file name
                // read all the bytes of the file name into a byte array
                // extract a string from the byte array using DOS (IBM OEM code page 437)
                // replace the unix forward slash with microsoft back slash
                fh.FileName = Encoding.GetEncoding(437)
                    .GetString(_streamReader.ReadBytes(fileNameLen)).Replace('/', '\\');
                // find if file name contains a path
                fh.Path = fh.FileName.Contains("\\");
                // if we have a directory, we must have a terminating slash
                if ((fh.FileAttr & FileAttributes.Directory) != 0 && !fh.Path)
                    throw new ApplicationException("Directory name must have a slash");

                // Skip Extra field and File comment
                _inputStream.Position += extraFieldLen + commentLen;

                // add file header to zip directory
                _zipDir.Add(fh);
            }

            // sort array
            _zipDir.Sort();
        }

        private string ReadFileHeader(uint headerPosition)
        {
            //
            //	file header
            //
            //	Pos		Len
            //	0		4		Local file header signature = 0x04034b50
            //	4		2		Version needed to extract (minimum)
            //	6		2		General purpose bit flag
            //	8		2		Compression method
            //	10		2		File last modification time
            //	12		2		File last modification date
            //	14		4		CRC-32
            //	18		4		Compressed size
            //	22		4		Uncompressed size
            //	26		2		File name length (n)
            //	28		2		Extra field length (m)
            //	30		n		File name
            //	30+n	m		Extra field
            //

            // set initial position
            _inputStream.Position = headerPosition;

            // local file header signature
            if (_streamReader.ReadUInt32() != 0x04034b50)
                throw new ApplicationException("Zip file signature in error");

            // NOTE: The program uses the file header information from the ZIP directory
            // at the end of the file. The local file header is ignored except for file times.
            // One can skip 22 bytes instead of reading these fields.
            // ReadStream.Position += 22;

            // version needed to extract and file system for external file attributes
            UInt16 version = _streamReader.ReadUInt16();
            UInt16 bitFlags = _streamReader.ReadUInt16();
            UInt16 compMethod = _streamReader.ReadUInt16();
            UInt16 fileTime = _streamReader.ReadUInt16();
            UInt16 fileDate = _streamReader.ReadUInt16();
            UInt32 fileCRC32 = _streamReader.ReadUInt32();
            if (_fileHeaderInfo.FileCRC32 != fileCRC32)
                throw new ApplicationException("File header error");

            UInt32 compSize = _streamReader.ReadUInt32();
            UInt32 fileSize = _streamReader.ReadUInt32();
            Int32 fileNameLen = _streamReader.ReadInt16();
            Int32 extraFieldLen = _streamReader.ReadInt16();

            // file name
            // read all the bytes of the file name into a byte array
            // extract a string from the byte array using DOS (IBM OEM code page 437)
            // replace the unix forward slash with microsoft back slash
            String fileName = Encoding.GetEncoding(437)
                .GetString(_streamReader.ReadBytes(fileNameLen)).Replace('/', '\\');

            // if extra field length is 36 get file times
            if (extraFieldLen == 36)
                _streamReader.ReadBytes(36);
            else // skip extra field
                _inputStream.Position += extraFieldLen;

            return fileName;
        }

        public override Int32 ReadBytes(Byte[] buffer, Int32 pos, Int32 len, out Boolean endOfFile)
        {
            len = len > _readRemain ? (Int32) _readRemain : len;
            _readRemain -= (UInt32) len;
            endOfFile = _readRemain == 0;
            return (_streamReader.Read(buffer, pos, len));
        }

        public override void WriteBytes(Byte[] buffer, Int32 pos, Int32 len)
        {
            _writeCRC32 = CRC32.Checksum(_writeCRC32, buffer, pos, len);
            _streamWriter.Write(buffer, pos, len);
        }
    }

    #region Nested classes

    internal static class BitReverse
    {
        private static readonly Byte[] BitReverseTable =
        {
            0x00, 0x80, 0x40, 0xc0, 0x20, 0xa0, 0x60, 0xe0, 0x10, 0x90, 0x50, 0xd0, 0x30, 0xb0, 0x70, 0xf0,
            0x08, 0x88, 0x48, 0xc8, 0x28, 0xa8, 0x68, 0xe8, 0x18, 0x98, 0x58, 0xd8, 0x38, 0xb8, 0x78, 0xf8,
            0x04, 0x84, 0x44, 0xc4, 0x24, 0xa4, 0x64, 0xe4, 0x14, 0x94, 0x54, 0xd4, 0x34, 0xb4, 0x74, 0xf4,
            0x0c, 0x8c, 0x4c, 0xcc, 0x2c, 0xac, 0x6c, 0xec, 0x1c, 0x9c, 0x5c, 0xdc, 0x3c, 0xbc, 0x7c, 0xfc,
            0x02, 0x82, 0x42, 0xc2, 0x22, 0xa2, 0x62, 0xe2, 0x12, 0x92, 0x52, 0xd2, 0x32, 0xb2, 0x72, 0xf2,
            0x0a, 0x8a, 0x4a, 0xca, 0x2a, 0xaa, 0x6a, 0xea, 0x1a, 0x9a, 0x5a, 0xda, 0x3a, 0xba, 0x7a, 0xfa,
            0x06, 0x86, 0x46, 0xc6, 0x26, 0xa6, 0x66, 0xe6, 0x16, 0x96, 0x56, 0xd6, 0x36, 0xb6, 0x76, 0xf6,
            0x0e, 0x8e, 0x4e, 0xce, 0x2e, 0xae, 0x6e, 0xee, 0x1e, 0x9e, 0x5e, 0xde, 0x3e, 0xbe, 0x7e, 0xfe,
            0x01, 0x81, 0x41, 0xc1, 0x21, 0xa1, 0x61, 0xe1, 0x11, 0x91, 0x51, 0xd1, 0x31, 0xb1, 0x71, 0xf1,
            0x09, 0x89, 0x49, 0xc9, 0x29, 0xa9, 0x69, 0xe9, 0x19, 0x99, 0x59, 0xd9, 0x39, 0xb9, 0x79, 0xf9,
            0x05, 0x85, 0x45, 0xc5, 0x25, 0xa5, 0x65, 0xe5, 0x15, 0x95, 0x55, 0xd5, 0x35, 0xb5, 0x75, 0xf5,
            0x0d, 0x8d, 0x4d, 0xcd, 0x2d, 0xad, 0x6d, 0xed, 0x1d, 0x9d, 0x5d, 0xdd, 0x3d, 0xbd, 0x7d, 0xfd,
            0x03, 0x83, 0x43, 0xc3, 0x23, 0xa3, 0x63, 0xe3, 0x13, 0x93, 0x53, 0xd3, 0x33, 0xb3, 0x73, 0xf3,
            0x0b, 0x8b, 0x4b, 0xcb, 0x2b, 0xab, 0x6b, 0xeb, 0x1b, 0x9b, 0x5b, 0xdb, 0x3b, 0xbb, 0x7b, 0xfb,
            0x07, 0x87, 0x47, 0xc7, 0x27, 0xa7, 0x67, 0xe7, 0x17, 0x97, 0x57, 0xd7, 0x37, 0xb7, 0x77, 0xf7,
            0x0f, 0x8f, 0x4f, 0xcf, 0x2f, 0xaf, 0x6f, 0xef, 0x1f, 0x9f, 0x5f, 0xdf, 0x3f, 0xbf, 0x7f, 0xff
        };

        // Reverse the bits of a 16 bit value.
        public static UInt16 Reverse16Bits(Int32 value)
        {
            return ((UInt16) ((BitReverseTable[value & 0xff] << 8) | BitReverseTable[(value >> 8) & 0xff]));
        }
    }

    internal static class CRC32
    {
        private static readonly UInt32[] CRC32Table =
        {
            0x00000000, 0x77073096, 0xee0e612c, 0x990951ba, 0x076dc419,
            0x706af48f, 0xe963a535, 0x9e6495a3, 0x0edb8832, 0x79dcb8a4,
            0xe0d5e91e, 0x97d2d988, 0x09b64c2b, 0x7eb17cbd, 0xe7b82d07,
            0x90bf1d91, 0x1db71064, 0x6ab020f2, 0xf3b97148, 0x84be41de,
            0x1adad47d, 0x6ddde4eb, 0xf4d4b551, 0x83d385c7, 0x136c9856,
            0x646ba8c0, 0xfd62f97a, 0x8a65c9ec, 0x14015c4f, 0x63066cd9,
            0xfa0f3d63, 0x8d080df5, 0x3b6e20c8, 0x4c69105e, 0xd56041e4,
            0xa2677172, 0x3c03e4d1, 0x4b04d447, 0xd20d85fd, 0xa50ab56b,
            0x35b5a8fa, 0x42b2986c, 0xdbbbc9d6, 0xacbcf940, 0x32d86ce3,
            0x45df5c75, 0xdcd60dcf, 0xabd13d59, 0x26d930ac, 0x51de003a,
            0xc8d75180, 0xbfd06116, 0x21b4f4b5, 0x56b3c423, 0xcfba9599,
            0xb8bda50f, 0x2802b89e, 0x5f058808, 0xc60cd9b2, 0xb10be924,
            0x2f6f7c87, 0x58684c11, 0xc1611dab, 0xb6662d3d, 0x76dc4190,
            0x01db7106, 0x98d220bc, 0xefd5102a, 0x71b18589, 0x06b6b51f,
            0x9fbfe4a5, 0xe8b8d433, 0x7807c9a2, 0x0f00f934, 0x9609a88e,
            0xe10e9818, 0x7f6a0dbb, 0x086d3d2d, 0x91646c97, 0xe6635c01,
            0x6b6b51f4, 0x1c6c6162, 0x856530d8, 0xf262004e, 0x6c0695ed,
            0x1b01a57b, 0x8208f4c1, 0xf50fc457, 0x65b0d9c6, 0x12b7e950,
            0x8bbeb8ea, 0xfcb9887c, 0x62dd1ddf, 0x15da2d49, 0x8cd37cf3,
            0xfbd44c65, 0x4db26158, 0x3ab551ce, 0xa3bc0074, 0xd4bb30e2,
            0x4adfa541, 0x3dd895d7, 0xa4d1c46d, 0xd3d6f4fb, 0x4369e96a,
            0x346ed9fc, 0xad678846, 0xda60b8d0, 0x44042d73, 0x33031de5,
            0xaa0a4c5f, 0xdd0d7cc9, 0x5005713c, 0x270241aa, 0xbe0b1010,
            0xc90c2086, 0x5768b525, 0x206f85b3, 0xb966d409, 0xce61e49f,
            0x5edef90e, 0x29d9c998, 0xb0d09822, 0xc7d7a8b4, 0x59b33d17,
            0x2eb40d81, 0xb7bd5c3b, 0xc0ba6cad, 0xedb88320, 0x9abfb3b6,
            0x03b6e20c, 0x74b1d29a, 0xead54739, 0x9dd277af, 0x04db2615,
            0x73dc1683, 0xe3630b12, 0x94643b84, 0x0d6d6a3e, 0x7a6a5aa8,
            0xe40ecf0b, 0x9309ff9d, 0x0a00ae27, 0x7d079eb1, 0xf00f9344,
            0x8708a3d2, 0x1e01f268, 0x6906c2fe, 0xf762575d, 0x806567cb,
            0x196c3671, 0x6e6b06e7, 0xfed41b76, 0x89d32be0, 0x10da7a5a,
            0x67dd4acc, 0xf9b9df6f, 0x8ebeeff9, 0x17b7be43, 0x60b08ed5,
            0xd6d6a3e8, 0xa1d1937e, 0x38d8c2c4, 0x4fdff252, 0xd1bb67f1,
            0xa6bc5767, 0x3fb506dd, 0x48b2364b, 0xd80d2bda, 0xaf0a1b4c,
            0x36034af6, 0x41047a60, 0xdf60efc3, 0xa867df55, 0x316e8eef,
            0x4669be79, 0xcb61b38c, 0xbc66831a, 0x256fd2a0, 0x5268e236,
            0xcc0c7795, 0xbb0b4703, 0x220216b9, 0x5505262f, 0xc5ba3bbe,
            0xb2bd0b28, 0x2bb45a92, 0x5cb36a04, 0xc2d7ffa7, 0xb5d0cf31,
            0x2cd99e8b, 0x5bdeae1d, 0x9b64c2b0, 0xec63f226, 0x756aa39c,
            0x026d930a, 0x9c0906a9, 0xeb0e363f, 0x72076785, 0x05005713,
            0x95bf4a82, 0xe2b87a14, 0x7bb12bae, 0x0cb61b38, 0x92d28e9b,
            0xe5d5be0d, 0x7cdcefb7, 0x0bdbdf21, 0x86d3d2d4, 0xf1d4e242,
            0x68ddb3f8, 0x1fda836e, 0x81be16cd, 0xf6b9265b, 0x6fb077e1,
            0x18b74777, 0x88085ae6, 0xff0f6a70, 0x66063bca, 0x11010b5c,
            0x8f659eff, 0xf862ae69, 0x616bffd3, 0x166ccf45, 0xa00ae278,
            0xd70dd2ee, 0x4e048354, 0x3903b3c2, 0xa7672661, 0xd06016f7,
            0x4969474d, 0x3e6e77db, 0xaed16a4a, 0xd9d65adc, 0x40df0b66,
            0x37d83bf0, 0xa9bcae53, 0xdebb9ec5, 0x47b2cf7f, 0x30b5ffe9,
            0xbdbdf21c, 0xcabac28a, 0x53b39330, 0x24b4a3a6, 0xbad03605,
            0xcdd70693, 0x54de5729, 0x23d967bf, 0xb3667a2e, 0xc4614ab8,
            0x5d681b02, 0x2a6f2b94, 0xb40bbe37, 0xc30c8ea1, 0x5a05df1b,
            0x2d02ef8d
        };

        public static UInt32 Checksum(UInt32 crc, Byte[] buffer, Int32 pos, Int32 len)
        {
            crc = ~crc;
            for (; len > 0; len--) crc = CRC32Table[(crc ^ buffer[pos++]) & 0xff] ^ (crc >> 8);
            return (~crc);
        }
    }

    internal class InflateMethod
    {
        private enum BlockType
        {
            StoredBlock,
            StaticTrees,
            DynamicTrees
        }

        // Window size is the maximum distance the repeated string matching process will look for a match
        private const Int32 WindowSize = 32768; // 0x8000

        // maximum number of codes for the three Huffman trees
        private const Int32 MaxLiteralCodes = 286;
        private const Int32 MaxDistanceCodes = 30;
        private const Int32 MaxBitLengthCodes = 19;

        // bit length repeast symbols
        private const Int32 RepeatSymbol_3_6 = 16;
        private const Int32 RepeatSymbol_3_10 = 17;
        private const Int32 RepeatSymbol_11_138 = 18;

        // Bit length symbols are transmitted in a coded way.
        // This array translate real codes to transmitted codes.
        // It is done to with the hope that most likely codes are at the begining
        // and the least likely will be at the end and if not used will not be transmitted.
        private static readonly Int32[] BitLengthOrder;

        // Base lengths for literal codes 257..285
        private static readonly Int32[] BaseLength;

        // Extra bits for literal codes 257..285
        private static readonly Int32[] ExtraLengthBits;

        // Base offsets for distance codes 0..29
        private static readonly Int32[] BaseDistance;

        // Extra bits for distance codes
        private static readonly Int32[] ExtraDistanceBits;

        private readonly Byte[] _readBuffer; // compressed data input buffer
        private Int32 _readPtr; // current pointer to compressed data item
        private Int32 _readBufEnd; // end of compressed data in the buffer
        private Boolean _readEndOfFile; // end of file flag. If true, the current read buffer is the last buffer
        private UInt32 _bitBuffer; // 32 bit buffer for reading for reading variable length bits codes
        private Int32 _bitCount; // bit buffer active bit count
        private const Int32 ReadBufSize = 0x100000; // read buffer length 1MB

        private readonly Byte[] _writeBuffer; // decompressed data buffer
        private Int32 _writePtr; // current pointer to end of the write data in the buffer
        private const Int32 WriteBufSize = 0x100000; // write buffer length 1MB

        // allocate arrays
        private readonly Byte[] _bitLengthArray;
        private readonly Byte[] _literalDistanceArray;

        private readonly InflateTree _bitLengthTree;
        private readonly InflateTree _literalTree;
        private readonly InflateTree _distanceTree;

        static InflateMethod()
        {
            // Bit length symbols are transmitted in a coded way.
            // This array translate real codes to transmitted codes.
            // It is done to with the hope that most likely codes are at the begining
            // and the least likely will be at the end and if not used will not be transmitted.
            BitLengthOrder = new[]
            {
                RepeatSymbol_3_6, RepeatSymbol_3_10, RepeatSymbol_11_138,
                0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15
            };

            // Length Code (See RFC 1951 3.2.5)
            //		 Extra               Extra               Extra
            //	Code Bits Length(s) Code Bits Lengths   Code Bits Length(s)
            //	---- ---- ------     ---- ---- -------   ---- ---- -------
            //	 257   0     3       267   1   15,16     277   4   67-82
            //	 258   0     4       268   1   17,18     278   4   83-98
            //	 259   0     5       269   2   19-22     279   4   99-114
            //	 260   0     6       270   2   23-26     280   4  115-130
            //	 261   0     7       271   2   27-30     281   5  131-162
            //	 262   0     8       272   2   31-34     282   5  163-194
            //	 263   0     9       273   3   35-42     283   5  195-226
            //	 264   0    10       274   3   43-50     284   5  227-257
            //	 265   1  11,12      275   3   51-58     285   0    258
            //	 266   1  13,14      276   3   59-66
            //
            // Base lengths for literal codes 257..285
            BaseLength = new[]
            {
                3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 15, 17, 19, 23, 27, 31,
                35, 43, 51, 59, 67, 83, 99, 115, 131, 163, 195, 227, 258
            };

            // Extra bits for literal codes 257..285
            ExtraLengthBits = new[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2,
                3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0
            };

            // Distance Codes (See RFC 1951 3.2.5)
            //		 Extra           Extra               Extra
            //	Code Bits Dist  Code Bits   Dist     Code Bits Distance
            //	---- ---- ----  ---- ----  ------    ---- ---- --------
            //	  0   0    1     10   4     33-48    20    9   1025-1536
            //	  1   0    2     11   4     49-64    21    9   1537-2048
            //	  2   0    3     12   5     65-96    22   10   2049-3072
            //	  3   0    4     13   5     97-128   23   10   3073-4096
            //	  4   1   5,6    14   6    129-192   24   11   4097-6144
            //	  5   1   7,8    15   6    193-256   25   11   6145-8192
            //	  6   2   9-12   16   7    257-384   26   12  8193-12288
            //	  7   2  13-16   17   7    385-512   27   12 12289-16384
            //	  8   3  17-24   18   8    513-768   28   13 16385-24576
            //	  9   3  25-32   19   8   769-1024   29   13 24577-32768
            //
            // Base offsets for distance codes 0..29
            BaseDistance = new[]
            {
                1, 2, 3, 4, 5, 7, 9, 13, 17, 25, 33, 49, 65, 97, 129, 193, 257, 385,
                513, 769, 1025, 1537, 2049, 3073, 4097, 6145, 8193, 12289, 16385, 24577
            };

            // Extra bits for distance codes
            ExtraDistanceBits = new[]
            {
                0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6,
                7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13
            };
        }

        public InflateMethod()
        {
            // allocate buffers
            _readBuffer = new Byte[ReadBufSize];
            _writeBuffer = new Byte[WriteBufSize];
            _bitLengthArray = new Byte[MaxBitLengthCodes];
            _literalDistanceArray = new Byte[MaxLiteralCodes + MaxDistanceCodes];
            _bitLengthTree = new InflateTree(TreeType.BitLen);
            _literalTree = new InflateTree(TreeType.Literal);
            _distanceTree = new InflateTree(TreeType.Distance);
        }

        public void Decompress()
        {
            // reset read process
            _readPtr = 0;
            _bitBuffer = 0;
            _bitCount = 0;

            // read first block
            _readBufEnd = ReadBytes(_readBuffer, 0, ReadBufSize, out _readEndOfFile);

            // reset write process
            _writePtr = 0;

            // reset last block flag
            Boolean lastBlock = false;

            // loop for blocks
            while (!lastBlock)
            {
                // get next block header
                Int32 blockHeader = GetBits(3);

                // set last block flag				
                if ((blockHeader & 1) != 0) lastBlock = true;

                // switch based on type of block
                switch ((BlockType) (blockHeader >> 1))
                {
                        // copy uncompressed block from read buffer to wrire buffer
                    case BlockType.StoredBlock:
                        CopyStoredBlock();
                        break;

                        // decode compressed block using static trees
                    case BlockType.StaticTrees:
                        _literalTree.SetStatic();
                        _distanceTree.SetStatic();
                        DecodeCompressedBlock();
                        break;

                        // decode compressed block using dynamic trees
                    case BlockType.DynamicTrees:
                        DecodeDynamicHuffamTrees();
                        _literalTree.SetDynamic();
                        _distanceTree.SetDynamic();
                        DecodeCompressedBlock();
                        break;

                    default:
                        throw new ApplicationException("Unknown block type");
                }
            }

            // flush write buffer
            WriteToFile(true);
        }

        public void NoCompression()
        {
            // loop writing directly from read buffer
            _readEndOfFile = false;
            while (!_readEndOfFile)
            {
                // read one block
                _readBufEnd = ReadBytes(_readBuffer, 0, ReadBufSize, out _readEndOfFile);

                // write read buffer content
                WriteBytes(_readBuffer, 0, _readBufEnd);
            }
        }

        private void CopyStoredBlock()
        {
            // move read buffer pointer to next multiple of 8 bits
            // drop bits that are not multiple of 8
            _bitBuffer >>= _bitCount & 7;

            // effectively subtract the dropped bits from the count
            _bitCount &= ~7;

            // get block length
            // note: the Get16Bits routine will empty the read bit buffer
            // after reading 2 blocks of 16 bits the bit buffer is guarantied to be empty
            Int32 blockLength = Get16Bits();

            // get the inverted block length and compare it to the block length
            if (Get16Bits() != (blockLength ^ 0xffff)) throw new ApplicationException("Stored block length in error");

            // make sure read buffer has enough bytes for full block transfer
            // not enough bytes available
            if (_readPtr + blockLength > _readBufEnd) TestReadBuffer(blockLength);

            // make sure write buffer has enough space to receive the block in one transfer
            if (_writePtr + blockLength > _writeBuffer.Length) WriteToFile(false);

            // write to output buffer
            Array.Copy(_readBuffer, _readPtr, _writeBuffer, _writePtr, blockLength);

            // update pointers and total
            _writePtr += blockLength;
            _readPtr += blockLength;
        }

        /// <summary>
        /// Get next 16 bits
        // Assume that bit buffer is aligned on 8 bit boundry.
        // Empty the bit buffer first. After two calles to this routine
        // the bit buffer is guarantied to be empty.
        /// </summary>
        private Int32 Get16Bits()
        {
            Int32 token;

            // bit buffer has 16 or 24 bits
            if (_bitCount >= 16)
            {
                token = (Int32) _bitBuffer & 0xffff;
                _bitBuffer >>= 16;
                _bitCount -= 16;
                return (token);
            }

            // bit buffer has 8 bits
            if (_bitCount >= 8)
            {
                // get high bits from bit buffer
                token = (Int32) _bitBuffer;
                _bitBuffer = 0;
                _bitCount = 0;
            }
            else
            {
                // get high bits from read buffer
                token = ReadByte();

                // we are at the end of file case
                if (token < 0) throw new ApplicationException("Unexpected end of file (Get16Bits)");
            }

            // get low bits from read buffer
            Int32 nextByte = ReadByte();

            // we are at the end of file case
            if (nextByte < 0) throw new ApplicationException("Unexpected end of file (Get16Bits)");

            // return 16 bits
            return ((nextByte << 8) | token);
        }

        private void TestReadBuffer(Int32 len)
        {
            // end of file flag is on
            if (_readEndOfFile) throw new ApplicationException("Premature end of file reading zip header");

            // move the top part of the file to the start of the buffer (round to 8 bytes)
            Int32 startOfMovePtr = _readPtr & ~7;
            Int32 moveSize = _readBufEnd - startOfMovePtr;
            Array.Copy(_readBuffer, startOfMovePtr, _readBuffer, 0, moveSize);

            // adjust read pointer
            _readPtr &= 7;

            // read one block
            _readBufEnd = moveSize + ReadBytes(_readBuffer, moveSize, ReadBufSize - moveSize, out _readEndOfFile);

            // test again for sufficient look ahead buffer
            if (_readPtr + len > _readBufEnd) throw new ApplicationException("Premature end of file reading zip header");

            // get next byte
        }

        private void DecodeCompressedBlock()
        {
            // loop for all symbols of one block
            for (;;)
            {
                // Loop while symbols are less than 256.
                // In other words input literals go unchanged to output stream
                Int32 symbol;
                while ((symbol = ReadSymbol(_literalTree)) < 256)
                {
                    // test for write buffer full
                    if (_writePtr == WriteBufSize) WriteToFile(false);

                    // write to output buffer
                    _writeBuffer[_writePtr++] = (Byte) symbol;
                }

                // end of block
                if (symbol == 256) return;

                // translate symbol into copy length
                symbol -= 257;
                Int32 strLength = BaseLength[symbol];
                Int32 reqBits = ExtraLengthBits[symbol];
                if (reqBits > 0) strLength += GetBits(reqBits);

                // get next symbol
                symbol = ReadSymbol(_distanceTree);

                // translate into copy distance
                Int32 strDist = BaseDistance[symbol];
                reqBits = ExtraDistanceBits[symbol];
                if (reqBits > 0) strDist += GetBits(reqBits);

                // test for write buffer full
                if (_writePtr + strLength > WriteBufSize) WriteToFile(false);

                // test for overlap
                Int32 len = strLength > strDist ? strDist : strLength;

                // write to output buffer
                Array.Copy(_writeBuffer, _writePtr - strDist, _writeBuffer, _writePtr, len);

                // update pointer and length
                _writePtr += len;

                // special case of repeating strings
                if (strLength > strDist)
                {
                    // copy one byte at a time
                    Int32 writeEnd = _writePtr + strLength - strDist;
                    for (; _writePtr < writeEnd; _writePtr++) _writeBuffer[_writePtr] = _writeBuffer[_writePtr - strDist];
                }
            }
        }

        private void WriteToFile(Boolean flush)
        {
            // write buffer keeping one window size (make sure it is multiple of 8)
            Int32 len = flush ? _writePtr : (_writePtr - WindowSize) & ~(7);

            // write to file
            WriteBytes(_writeBuffer, 0, len);

            // move leftover to start of buffer (except for flush)
            _writePtr -= len;
            Array.Copy(_writeBuffer, len, _writeBuffer, 0, _writePtr);
        }

        private void DecodeDynamicHuffamTrees()
        {
            // length of length/literal tree		
            Int32 LiteralLength = GetBits(5) + 257;

            // length of distance tree
            Int32 distanceLength = GetBits(5) + 1;

            // length of bit length tree
            Int32 bitLengthReceived = GetBits(4) + 4;

            // get bit length info from input stream
            // note: array length must be 19 and not length received
            Array.Clear(_bitLengthArray, 0, _bitLengthArray.Length);
            for (Int32 index = 0; index < bitLengthReceived; index++)
                _bitLengthArray[BitLengthOrder[index]] = (Byte) GetBits(3);

            // create bit length hauffman tree
            _bitLengthTree.BuildTree(_bitLengthArray, 0, _bitLengthArray.Length);

            // create a combined array of length/literal and distance
            Int32 totalLength = LiteralLength + distanceLength;
            Array.Clear(_literalDistanceArray, 0, _literalDistanceArray.Length);
            Byte lastCode = 0;
            for (Int32 ptr = 0; ptr < totalLength;)
            {
                // get next symbol from input stream
                Int32 symbol = ReadSymbol(_bitLengthTree);

                // switch based on symbol
                switch (symbol)
                {
                        // symbol is less than 16 it is a literal
                    default:
                        _literalDistanceArray[ptr++] = lastCode = (Byte) symbol;
                        continue;

                    case RepeatSymbol_3_6:
                        for (var count = GetBits(2) + 3; count > 0; count--) _literalDistanceArray[ptr++] = lastCode;
                        continue;

                    case RepeatSymbol_3_10:
                        for (Int32 count = GetBits(3) + 3; count > 0; count--) _literalDistanceArray[ptr++] = 0;
                        continue;

                    case RepeatSymbol_11_138:
                        for (Int32 count = GetBits(7) + 11; count > 0; count--) _literalDistanceArray[ptr++] = 0;
                        continue;
                }
            }

            // create the literal array and distance array
            _literalTree.SetDynamic();
            _literalTree.BuildTree(_literalDistanceArray, 0, LiteralLength);
            _distanceTree.SetDynamic();
            _distanceTree.BuildTree(_literalDistanceArray, LiteralLength, distanceLength);
        }

        private Int32 ReadByte()
        {
            // test for end of read buffer
            if (_readPtr == _readBufEnd)
            {
                // end of file flag was set during last read operation
                if (_readEndOfFile) return (-1);

                // read one block
                _readBufEnd = ReadBytes(_readBuffer, 0, ReadBufSize, out _readEndOfFile);

                // test again for sufficient look ahead buffer
                if (_readBufEnd == 0) throw new ApplicationException("Premature end of file reading zip header");

                // reset read pointer
                _readPtr = 0;
            }

            // get next byte
            return _readBuffer[_readPtr++];
        }

        private Int32 GetBits(Int32 bits)
        {
            // fill the buffer to a maximum of 32 bits
            for (; _bitCount <= 24; _bitCount += 8)
            {
                Int32 oneByte = ReadByte();
                if (oneByte < 0) break;
                _bitBuffer |= (UInt32) (oneByte << _bitCount);
            }

            // error: the program should not ask for bits beyond end of file
            if (bits > _bitCount) throw new ApplicationException("Peek Bits: Premature end of file");

            Int32 token = (Int32) _bitBuffer & ((1 << bits) - 1);
            _bitBuffer >>= bits;
            _bitCount -= bits;
            return (token);
        }

        private Int32 ReadSymbol(InflateTree tree)
        {
            // fill the buffer to a maximum of 32 bits
            for (; _bitCount <= 24; _bitCount += 8)
            {
                // read next byte from read buffer
                Int32 oneByte = ReadByte();

                // end of file
                if (oneByte < 0) break;

                // append to the bit buffer
                _bitBuffer |= (UInt32) (oneByte << _bitCount);
            }

            // loop through the decode tree
            Int32 next, mask;
            for (mask = tree.ActiveBitMask, next = tree.ActiveTree[(Int32) _bitBuffer & (mask - 1)];
                next > 0 && mask < 0x10000;
                next = (_bitBuffer & mask) == 0 ? tree.ActiveTree[next] : tree.ActiveTree[next + 1], mask <<= 1) ;

            // error
            if (next >= 0)
                throw new ApplicationException("Error decoding the compressed bit stream (decoding tree error)");

            // invert the symbol plus bit count
            next = ~next;

            // extract the number of bits
            Int32 bits = next & 15;

            // remove the bits from the bit buffer
            _bitBuffer >>= bits;
            _bitCount -= bits;

            // error
            if (_bitCount < 0)
                throw new ApplicationException("Error decoding the compressed bit stream (premature end of file)");

            // exit with symbol
            return (next >> 4);
        }

        public virtual Int32 ReadBytes(Byte[] buffer, Int32 pos, Int32 len, out Boolean endOfFile)
        {
            throw new ApplicationException("ReadBytes routine is missing");
        }

        public virtual void WriteBytes(Byte[] buffer, Int32 pos, Int32 len)
        {
            throw new ApplicationException("WriteBytes routine is missing");
        }
    }

    internal enum TreeType
    {
        Literal,
        Distance,
        BitLen
    }

    internal class InflateTree
    {
        private const Int32 MaxBitlen = 15;

        public Int32[] ActiveTree;
        public Int32 ActiveBitMask;
        private readonly Int32[] _staticTree;
        private readonly Int32 _staticBitMask;
        private readonly Int32[] _dynamicTree;
        private readonly Int32[] _bitLengthCount = new Int32[MaxBitlen + 1];
        private readonly Int32[] _initialCode = new Int32[MaxBitlen + 1];

        public InflateTree(TreeType type)
        {
            Byte[] codeLength;

            // switch based on type
            switch (type)
            {
                    // literal tree
                case TreeType.Literal:
                    // literal tree is made of 286 codes alphabet however inorder to balance the tree it is made of 288
                    codeLength = new Byte[288];

                    // build code length for static literal tree
                    Int32 index = 0;

                    // the first 144 codes are 8 bits long
                    while (index < 144) codeLength[index++] = 8;

                    // the next 112 codes are 9 bits long
                    while (index < 256) codeLength[index++] = 9;

                    // the next 24 codes are 7 bits long
                    while (index < 280) codeLength[index++] = 7;

                    // the last 8 codes are 8 bits long
                    while (index < 288) codeLength[index++] = 8;

                    // build the static tree
                    ActiveTree = new Int32[512];
                    BuildTree(codeLength, 0, codeLength.Length);
                    _staticTree = ActiveTree;
                    _staticBitMask = ActiveBitMask;

                    // alocate the space for dynamic tree
                    _dynamicTree = new Int32[1024];
                    break;

                    // distance tree
                case TreeType.Distance:
                    // distance tree is made of 32 codes alphabet
                    codeLength = new Byte[32];

                    // all codes are 5 bits long
                    for (index = 0; index < 32; index++) codeLength[index] = 5;

                    // build the static tree
                    ActiveTree = new Int32[32];
                    BuildTree(codeLength, 0, codeLength.Length);
                    _staticTree = ActiveTree;
                    _staticBitMask = ActiveBitMask;

                    // alocate the dynamic tree
                    _dynamicTree = new Int32[575];
                    break;

                    // bit length tree
                case TreeType.BitLen:
                    ActiveTree = new Int32[128];
                    break;
            }
        }

        public void SetStatic()
        {
            ActiveTree = _staticTree;
            ActiveBitMask = _staticBitMask;
        }

        public void SetDynamic()
        {
            ActiveTree = _dynamicTree;
        }

        public void BuildTree(Byte[] codeLengths, Int32 offset, Int32 len)
        {
            // Build frequency array for bit length 1 to 15
            // Note: BitLengthCount[0] will never be used.
            // All elements of CodeLength are greater than zero.
            // In other words no code has zero length
            Array.Clear(_bitLengthCount, 0, _bitLengthCount.Length);
            for (Int32 index = 0; index < len; index++) _bitLengthCount[codeLengths[offset + index]]++;

            // build array of inital codes for each group of equal code length
            Int32 initCode = 0;
            for (Int32 bits = 1; bits <= MaxBitlen; bits++)
            {
                _initialCode[bits] = initCode;
                initCode += _bitLengthCount[bits] << (16 - bits);
            }

            // Warning
            // If BitLengthCount array was constructed properly InitCode should be equal to 65536.
            // During all my initial testing of decompressing ZIP archives coming from other programs
            // that was the case. I finally run into one file that was compressed by old version
            // of PKZIP version 2.50 4-15-1998 that the following commented statement threw exception.
            // I would strongly recomment to anyone making modification to the compression/decompression
            // software to activate this statement during the testing.
            //
            //if(InitCode != 65536) throw new ApplicationException("Code lengths don't add up properly.");
            //

            // longest code bit count
            Int32 maxBits;
            for (maxBits = MaxBitlen; _bitLengthCount[maxBits] == 0; maxBits--) ;

            // number of hash bits at the root of the decode tree
            // the longest code but no more than 9 bits
            Int32 activeHashBits = Math.Min(9, maxBits);

            // the decoding process is using this bit mask
            ActiveBitMask = 1 << activeHashBits;

            // hash table at the begining of the tree
            Array.Clear(ActiveTree, 0, ActiveTree.Length);

            // pointer to the area above the hash table for codes longer than hash bits
            Int32 endPtr = ActiveBitMask;

            // fill the tree
            for (Int32 index = 0; index < len; index++)
            {
                // code length in bits
                Int32 bits = codeLengths[offset + index];

                // code not in use
                if (bits == 0) continue;

                // reverse the code from the most significant part to the least significant part of the integer
                Int32 code = BitReverse.Reverse16Bits(_initialCode[bits]);

                // the number of bits is less than the hash bits
                // we need to add artificial entries for the missing bits
                if (bits < activeHashBits)
                {
                    Int32 nextInc = (1 << bits);
                    for (Int32 next = code; next < ActiveBitMask; next += nextInc)
                        ActiveTree[next] = ~(index << 4 | bits);
                }
                    // the number of bits is equal to the hash bits
                else if (bits == activeHashBits)
                    ActiveTree[code] = ~(index << 4 | bits);

                    // the number of bits is greater than the hash bits
                else
                {
                    // hash pointer to the start of the tree table
                    Int32 hashPtr = code & (ActiveBitMask - 1);

                    // get the value at this location
                    Int32 next = ActiveTree[hashPtr];

                    // the location is not initialized
                    if (next == 0)
                    {
                        // get a free node at the area above the hash area and link it to the tree
                        ActiveTree[hashPtr] = endPtr;
                        next = endPtr;
                        endPtr += 2;
                    }

                    // navigate through the tree above the hash area
                    // add empty nodes as required
                    Int32 bitMaskEnd = 1 << (bits - 1);
                    for (Int32 bitMask = ActiveBitMask; bitMask != bitMaskEnd; bitMask <<= 1)
                    {
                        // current bit is one, adjust the next pointer
                        if ((code & bitMask) != 0) next++;

                        // get the value at this location
                        Int32 next1 = ActiveTree[next];

                        // the location was initialized before, continue to follow the path
                        if (next1 > 0)
                        {
                            next = next1;
                            continue;
                        }

                        // add free node from the area above the hash table and link it to the tree
                        ActiveTree[next] = endPtr;
                        next = endPtr;
                        endPtr += 2;
                    }

                    // we are now at a leaf point, add the symbol and the number of bits
                    if ((code & bitMaskEnd) != 0) next++;
                    ActiveTree[next] = ~(index << 4 | bits);
                }

                // update initial code for the next code with the same number of bits
                _initialCode[bits] += 1 << (16 - bits);
            }
        }
    }

    internal enum FileSystem
    {
        DOS,
        Amiga,
        OpenVMS,
        Unix,
        VMCMS,
        Atari,
        OS2,
        Mac,
        ZSys,
        CPM,
        NTFS,
        MVS,
        VSE,
        Acorn,
        VFAT,
        ALTMVS,
        BeOS,
        Tandem,
        OS400,
        OSX,
        Unused,
    };

    internal class FileHeader : IComparable<FileHeader>
    {
        public String FileName; // file name with or without path
        // file name will not start with drive letter or server name
        // in other words D:\ or \ or \\server-name\ are invalid
        public Boolean Path; // File name contains path
        public UInt32 FilePos; // file header position within the zip file
        public UInt16 FileTime; // file time in dos format hhhhhmmmmmmsssss (seconds are in 2 sec increments)
        public UInt16 FileDate; // file date in dos format yyyyyyymmmmddddd (years since 1980)
        public FileAttributes FileAttr; // file attributes (read only=1, hidden=2, system= 4, directory=8)
        public UInt32 FileSize; // uncompressed file size (4GB max)
        public UInt32 CompSize; // compressed file size. This nnumber does not include the file header.
        public UInt32 FileCRC32; // uncompressed file CRC32 checksum
        public UInt16 CompMethod; // compression method. This program supports 0-no compression and 8-deflate method

        public UInt16 BitFlags;
            // This program expects zero (see PKWARE ZIP file format specifications section "J. Explanation of fields"

        public UInt16 Version; // low byte=version, high byte=file system

        public FileHeader()
        {
        }

        public FileHeader(String fileName, DateTime lastWriteTime, FileAttributes fileAttr, Int64 filePos,
            Int64 fileSize)
        {
            if (fileName[0] == '\\' || fileName[1] == ':')
                throw new ApplicationException("Invalid file name");
            FileName = fileName;
            Path = FileName.Contains("\\");
            FileTime = (UInt16) ((lastWriteTime.Hour << 11) | (lastWriteTime.Minute << 5) | (lastWriteTime.Second/2));
            FileDate = (UInt16) (((lastWriteTime.Year - 1980) << 9) | (lastWriteTime.Month << 5) | lastWriteTime.Day);
            FileAttr = fileAttr & (FileAttributes.Archive | FileAttributes.Directory |
                                   FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System);
            if (filePos > 0xffffffff) throw new ApplicationException("No support for files over 4GB");
            FilePos = (UInt32) filePos;
            if (fileSize > 0xffffffff) throw new ApplicationException("No support for files over 4GB");
            FileSize = (UInt32) fileSize;
            Version = 20;
        }

        public Int32 CompareTo(FileHeader other)
        {
            if (Path != other.Path) return (Path ? 1 : -1);
            return (String.CompareOrdinal(FileName, other.FileName));
        }

        public static Int32 CompareByPosition(FileHeader one, FileHeader other)
        {
            // this should not happen
            if (one.FilePos == other.FilePos) return (0);

            // FilePos is UInt32 this is the reason for not using One.FilePos - Other.FilePos
            return (one.FilePos > other.FilePos ? 1 : -1);
        }
    }

    #endregion
}