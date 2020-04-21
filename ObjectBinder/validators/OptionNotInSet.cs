﻿using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.CommandLine
{
    public class OptionNotInSet<T> : IOptionValidator<T>
    {
        private List<T> _checkValues;

        public OptionNotInSet(params T[] checkValues)
        {
            _checkValues = new List<T>(checkValues);
        }

        public OptionNotInSet(List<T> checkValues)
        {
            _checkValues = checkValues;
        }

        public bool IsValid(T toCheck) => !_checkValues.Any(x => x.Equals(toCheck));

        public string GetErrorMessage( string optionName, T toCheck) => IsValid(toCheck)
            ? null
            : $"{optionName}: {toCheck} is one of the not allowed values  {string.Join(",", _checkValues.Select(x => x.ToString()))}";
    }
}