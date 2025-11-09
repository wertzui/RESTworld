using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoFixture.Kernel;

/// <summary>
/// Contains extensions for the reflection of a property picker expression.
/// </summary>
public static class ExpressionExtensions
{
    /// <summary>
    /// Gets the writable member from the given property expression.
    /// </summary>
    /// <param name="propertyPicker">The property picker.</param>
    /// <returns></returns>
    /// <exception cref="System.ArgumentException">
    /// The expression's Body is not a MemberExpression. " +
    ///                     "Most likely this is because it does not represent access to a property or field. - propertyPicker
    /// or
    /// propertyPicker
    /// </exception>
    /// <remarks>Same as https://github.com/AutoFixture/AutoFixture/blob/master/Src/AutoFixture/Kernel/ExpressionReflector.cs, but public.</remarks>
    public static MemberExpression GetWritableMember(this LambdaExpression propertyPicker)
    {
        var bodyExpr = propertyPicker.Body;

        // Method from C# may lead to an implicit conversion - e.g. if the property or field type is System.Byte,
        // but the supplied value is a System.Int32 value. Since there's an implicit conversion, the resulting
        // expression may be (x => Convert(x.Property)) and we need to unwrap it.
        if (bodyExpr.UnwrapIfConversionExpression() is not MemberExpression memberExpr)
        {
            throw new ArgumentException(
                "The expression's Body is not a MemberExpression. " +
                "Most likely this is because it does not represent access to a property or field.",
                nameof(propertyPicker));
        }

        if (memberExpr.Member is PropertyInfo pi && pi.GetSetMethod() == null)
        {
            throw new ArgumentException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    "The property \"{0}\" is read-only.", pi.Name),
                nameof(propertyPicker));
        }
        return memberExpr;
    }

    /// <summary>
    /// If current expression is a conversion expression, unwrap it and return the underlying expression.
    /// Otherwise, do nothing.
    /// </summary>
    /// <remarks>Same as https://github.com/AutoFixture/AutoFixture/blob/master/Src/AutoFixture/Kernel/ExpressionReflector.cs, but public.</remarks>
    public static Expression UnwrapIfConversionExpression(this Expression exp)
    {
        if (exp is UnaryExpression convExpr && convExpr.NodeType == ExpressionType.Convert)
            return convExpr.Operand;

        return exp;
    }
}