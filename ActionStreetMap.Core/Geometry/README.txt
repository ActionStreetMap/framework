This namespace contains third-party polygon specific libraries with some custom tweaks:

* Triangle.NET: https://triangle.codeplex.com/
	Version: Beta 4
	Datestamp: 1 Apr 2015
	Changes: renamed namespace, removed logging, removed unneeded logic, performance optimizations

* Clipper: http://www.angusj.com/delphi/clipper.php
	Version: 6.4.0
	Datestamp: 2 Jul 2014

* StraightSkeleton: https://github.com/kendzi/kendzi-math/tree/master/kendzi-straight-skeleton
	Datestamp: Dec 11, 2014
	Changes: My port from Java with numerous refactoring
	Info: Implementation of straight skeleton algorithm for polygons with holes. It is based on concept of 
	 tracking bisector intersection with queue of events to process and circular list with processed events
	 called lavs. This implementation is highly modified concept described by Petr Felkel and Stepan 
	 Obdrzalek [1]. In compare to original this algorithm has new kind of event and support for multiple 
	 events which appear in the same distance from edges. It is common when processing degenerate cases 
	 caused by polygon with right angles.
