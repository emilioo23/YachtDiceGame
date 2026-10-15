using System;

namespace YachtDice.Server.Services.Mapping
{
    /// <summary>
    /// Converts an enumeration value into the enumeration value with the same name.
    /// </summary>
    internal static class EnumMapper
    {
        /// <summary>
        /// Converts the value into the member of the target enumeration that has the same name.
        /// </summary>
        /// <typeparam name="TSource">Enumeration of the logic layer.</typeparam>
        /// <typeparam name="TTarget">Enumeration of the contract.</typeparam>
        /// <param name="value">Value to convert.</param>
        /// <returns>The member of the target enumeration with the same name.</returns>
        public static TTarget Map<TSource, TTarget>(TSource value)
            where TSource : struct, Enum
            where TTarget : struct, Enum
        {
            return (TTarget)Enum.Parse(typeof(TTarget), value.ToString());
        }
    }
}