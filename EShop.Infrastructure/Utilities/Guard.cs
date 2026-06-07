using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EShop.Infrastructure.Utilities;

public static partial class Guard
{
    internal const string NotEmptyStringMessage = "String parameter '{0}' can't be null or be a white space.";
    internal const string NotNegativeMessage = "Number parameter '{0}' can't be zero.";
    
    /// <summary>
    /// Asserts that the input value is not <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The type of reference value type being tested.</typeparam>
    /// <param name="value">The input value to test.</param>
    /// <param name="name">The name of the input parameter being tested.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotNull<T>([NotNull] T? value, [CallerArgumentExpression(nameof(value))] string name = "")
    {
        if (value is not null)
        {
            return;
        }
      
        throw new ArgumentNullException(name);
    }

    /// <summary>
    /// Asserts that the input value is not <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The type of nullable value type being tested.</typeparam>
    /// <param name="value">The input value to test.</param>
    /// <param name="name">The name of the input parameter being tested.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotNull<T>([NotNull] T? value, [CallerArgumentExpression(nameof(value))] string name = "")
        where T : struct
    {
        if (value is not null)
        {
            return;
        }

        throw new ArgumentNullException(name);
    }
    
    public static void NotEmpty(string? arg, [CallerArgumentExpression(nameof(arg))] string? argName = null)
    {
        if (arg is null)
        {
            throw new ArgumentNullException(argName);
        }
        else if (arg.Trim().Length == 0)
        {
            throw new ArgumentException(string.Format(NotEmptyStringMessage, argName), argName);
        }
    }
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T NotNegative<T>(T arg, [CallerArgumentExpression(nameof(arg))] string? argName = null, string message = NotNegativeMessage) where T : struct, IComparable<T>
    {
        if (arg.CompareTo(default) < 0)
        {
            throw new ArgumentOutOfRangeException(argName, string.Format(message, argName, arg));
        }

        return arg;
    }
}