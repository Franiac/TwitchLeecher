using System;
using System.Linq.Expressions;
using System.Reflection;

namespace TwitchLeecher.Shared.Commands
{
    public static class PropertySupport
    {
        public static string ExtractPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException(nameof(propertyExpression));
            }

            return ExtractPropertyNameFromLambda(propertyExpression);
        }

        internal static string ExtractPropertyNameFromLambda(LambdaExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            MemberExpression memberExpression = expression.Body as MemberExpression;

            if (memberExpression == null)
            {
                throw new ArgumentException("The expression is not a member access expression", nameof(expression));
            }

            PropertyInfo property = memberExpression.Member as PropertyInfo;

            if (property == null)
            {
                throw new ArgumentException("The member access expression does not access a property", nameof(expression));
            }

            MethodInfo getMethod = property.GetMethod;

            if (getMethod.IsStatic)
            {
                throw new ArgumentException("The referenced property is a static property", nameof(expression));
            }

            return memberExpression.Member.Name;
        }
    }
}