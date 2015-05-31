using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Core.Utils;
using ActionStreetMap.Explorer.Commands;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Maps.Entities;
using ActionStreetMap.Maps.Helpers;
using UnityEngine;

namespace ActionStreetMap.Explorer.Interactions
{
    public class AddressLocatorBehaviour: MonoBehaviour
    {
        private const string CommandName = "search";
        private readonly Subject<Address> _subject = new Subject<Address>();

        public string Tag = "addr:street";
        public float Distance = 200f;
        public float Interval = 3;

        private SearchCommand _searchCommand;

        public AddressLocatorBehaviour SetCommandController(CommandController controller)
        {
            _searchCommand = controller[CommandName] as SearchCommand;
            return this;
        }

        public IObservable<Address> GetObservable()
        {
            return _subject;
        }

        private void Start()
        {
            Observable
                .Interval(TimeSpan.FromSeconds(Interval))
                .ObserveOn(Scheduler.ThreadPool)
                .Subscribe(_ => ExtractAddress());
        }

        private void ExtractAddress()
        {
            var elements = _searchCommand.GetElementsByText(Tag, Distance)
                .ToArray()
                .Wait();

            if (elements == null || !elements.Any() || elements[0] == null)
                return;
            var coordinate = _searchCommand.SearchCenter;
            Array.Sort(elements, new ElementDistanceComparer(coordinate));
            _subject.OnNext(AddressExtractor.Extract(elements[0].Tags));
        }

        private class ElementDistanceComparer: IComparer<Element>
        {
            private readonly GeoCoordinate _position;

            public ElementDistanceComparer(GeoCoordinate position)
            {
                _position = position;
            }

            public int Compare(Element x, Element y)
            {
                return GetDistance(x).CompareTo(GetDistance(y));
            }

            private double GetDistance(Element element)
            {
                if (element is Node)
                    return GeoProjection.Distance((element as Node).Coordinate, _position);
                if (element is Way)
                    return (element as Way).Coordinates.Min(geoCoordinate =>
                    GeoProjection.Distance(geoCoordinate, _position));
                return (element as Relation).Members.Min(member => GetDistance(member.Member));
            }
        }
    }
}
