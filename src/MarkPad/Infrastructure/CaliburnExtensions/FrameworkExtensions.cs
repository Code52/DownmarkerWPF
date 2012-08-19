namespace Caliburn.Micro
{
    #region Namespaces
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq.Expressions;
    using System.Windows;
    using System.Windows.Markup;
    using System.Xml;
    using DynamicExpression = System.Linq.Dynamic.DynamicExpression;
    using Expression = System.Linq.Expressions.Expression;

    #endregion

    /// <summary>
    ///   Static class used to store Caliburn.Micro extensions.
    /// </summary>
    public static class FrameworkExtensions
    {
        #region Nested type: Message
        /// <summary>
        ///   Static class used to store extensions related to the <see cref = "Caliburn.Micro.Message" />.
        /// </summary>
        public static class Message
        {
            #region Nested type: Attach
            /// <summary>
            ///   Static class used to store extensions related to the <see cref = "Caliburn.Micro.Message.AttachProperty" />.
            /// </summary>
            public static class Attach
            {
                #region Static Fields
                /// <summary>
                ///   The additional namespaces to be used when trying to parse an action parameter defined into a markup extension.
                /// </summary>
                public static string XamlNamespaces = "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" xmlns:cm=\"clr-namespace:Caliburn.Micro;assembly=Caliburn.Micro\"";

                /// <summary>
                ///   The default implementation of the <see cref = "Parser.CreateParameter" />.
                /// </summary>
                private static readonly Func<DependencyObject, string, Parameter> m_BaseCreateParameter = Parser.CreateParameter;

                /// <summary>
                ///   The default implementation of the <see cref = "MessageBinder.EvaluateParameter" />.
                /// </summary>
                private static readonly Func<string, Type, ActionExecutionContext, object> m_BaseEvaluateParameter = MessageBinder.EvaluateParameter;
                #endregion

                #region Static Members
                /// <summary>
                ///   The fragment used to generate a parameter.
                /// </summary>
                private const string PARAMETER_XAML_FRAGMENT = "<cm:Parameter {0} Value=\"{1}\"/>";

                /// <summary>
                ///   Loads the specified parameter using a <see cref = "XamlReader" />.
                /// </summary>
                /// <param name = "parameter">The parameter.</param>
                /// <returns>The deserialized object.</returns>
                private static object LoadXaml(string parameter)
                {
#if SILVERLIGHT
                    return XamlReader.Load(parameter);
#else
                    using (var input = new StringReader(parameter))
                    {
                        using (var reader = XmlReader.Create(input))
                            return XamlReader.Load(reader);
                    }
#endif
                }

                /// <summary>
                ///   Evaluates the parameter.
                /// </summary>
                /// <param name = "expression">The expression.</param>
                /// <param name = "context">The context.</param>
                /// <param name = "resultType">Type of the result.</param>
                /// <returns>The evaluated parameter.</returns>
                private static object EvaluateParameter(string expression, ActionExecutionContext context, Type resultType)
                {
                    try
                    {
                        var index = 0;
                        var parameters = new ParameterExpression[MessageBinder.SpecialValues.Count];
                        var values = new object[MessageBinder.SpecialValues.Count];
                        foreach (var pair in MessageBinder.SpecialValues)
                        {
                            var name = "@" + index;
                            expression = expression.Replace(pair.Key, name);
                            var value = pair.Value(context);
                            parameters[index] = Expression.Parameter(GetParameterType(value), name);
                            values[index] = value;
                            index++;
                        }

                        var exp = DynamicExpression.ParseLambda(parameters, resultType, expression);
                        return exp.Compile().DynamicInvoke(values);
                    }
                    catch (Exception exc)
                    {
                        LogManager.GetLog(typeof(MessageBinder)).Error(exc);
                        return null;
                    }
                }

                /// <summary>
                ///   Gets the parameter type.
                /// </summary>
                /// <param name = "value">The value.</param>
                /// <returns>The parameter type.</returns>
                private static Type GetParameterType(object value)
                {
                    return value != null ? value.GetType() : typeof(object);
                }

                /// <summary>
                ///   Allows action parameters to be specified using Xaml compact syntax and (optionally) parameters evaluation.
                /// </summary>
                /// <param name = "enableEvaluation">If set to <c>true</c> action parameters will be evaluated, if needed.</param>
                public static void AllowXamlSyntax(bool enableEvaluation = true)
                {
                    if (enableEvaluation)
                    {
                        Parser.CreateParameter = (target, parameterText) =>
                        {
                            //Check if the parameter is defined as a markup...
                            if (parameterText.StartsWith("{") && parameterText.EndsWith("}"))
                            {
                                try
                                {
                                    parameterText = string.Format(PARAMETER_XAML_FRAGMENT, XamlNamespaces, parameterText);
                                    var parsed = LoadXaml(parameterText);

                                    return (Parameter)parsed;
                                }
                                catch (Exception exc)
                                {
                                    LogManager.GetLog(typeof(Parser)).Error(exc);
                                }
                            }

                            //Pass the textual value and let it be evaluated afterwards...
                            return new Parameter
                            {
                                Value = parameterText
                            };
                        };

                        MessageBinder.EvaluateParameter = (text, parameterType, context) =>
                        {
                            var lookup = text.ToLower(CultureInfo.InvariantCulture);
                            Func<ActionExecutionContext, object> resolver;

                            return MessageBinder.SpecialValues.TryGetValue(lookup, out resolver) ? resolver(context) : (typeof(string) == parameterType ? text : EvaluateParameter(text, context, parameterType));
                        };
                    }
                    else
                    {
                        Parser.CreateParameter = (target, parameterText) =>
                        {
                            //Check if the parameter is defined as a markup...
                            if (parameterText.StartsWith("{") && parameterText.EndsWith("}"))
                            {
                                try
                                {
                                    parameterText = string.Format(PARAMETER_XAML_FRAGMENT, XamlNamespaces, parameterText);
                                    var parsed = LoadXaml(parameterText);

                                    return (Parameter)parsed;
                                }
                                catch (Exception exc)
                                {
                                    LogManager.GetLog(typeof(Parser)).Error(exc);
                                }
                            }

                            //Use the default implementation if the parameter is not identified as a binding...
                            return m_BaseCreateParameter(target, parameterText);
                        };

                        MessageBinder.EvaluateParameter = m_BaseEvaluateParameter;
                    }
                }
                #endregion
            }
            #endregion
        }
        #endregion
    }
}
