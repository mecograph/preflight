﻿using Umbraco.Core;
using Umbraco.Core.Strings;

namespace Preflight.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Is the string not null and not empty?
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool HasValue(this string str)
        {
            return !string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// camelCaseTheString
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Camel(this string str)
        {
            return str.ToCleanString(CleanStringType.CamelCase);
        }

        /// <summary>
        /// Appends 'Disabled' and camelCases the given string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string OnSaveOnlyAlias(this string str)
        {
            return $"{str} on save only".Camel();
        }

        /// <summary>
        /// Appends 'Disabled' and camelCases the given string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string DisabledAlias(this string str)
        {
            return $"{str} disabled".Camel();
        }
    }
}
