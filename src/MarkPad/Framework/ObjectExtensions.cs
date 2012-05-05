using System;

namespace MarkPad.Framework
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Returns the result of <paramref name="func"/> if <paramref name="obj"/> is not null.
        /// <example>
        /// <code>
        /// Request.Url.Evaluate(x => x.Query)
        /// </code>
        /// </example>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="obj">The obj.</param>
        /// <param name="func">The func.</param>
        /// <returns></returns>
        public static TResult Evaluate<T, TResult>(this T obj, Func<T, TResult> func) where T : class
        {
            return Evaluate(obj, func, default(TResult));
        }

        /// <summary>
        /// Returns the result of <paramref name="func"/> if <paramref name="obj"/> is not null.
        /// Otherwise, <paramref name="defaultValue"/> is returned.
        /// <example>
        /// <code>
        /// Request.Url.Evaluate(x => x.Query, "default")
        /// </code>
        /// </example>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="obj"></param>
        /// <param name="func"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static TResult Evaluate<T, TResult>(this T obj, Func<T, TResult> func, TResult defaultValue) where T : class
        {
            return obj != null ? func(obj) : defaultValue;
        }

        /// <summary>
        /// Executes an action if <paramref name="obj"/> is not null, otherwise does nothing
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">An object of type <typeparamref name="T"/>.</param>
        /// <param name="action">The action to perform if <paramref name="obj"/> is not null</param>
        /// <returns><paramref name="obj"/> to allow for chaining</returns>
        public static T ExecuteSafely<T>(this T obj, Action<T> action) where T : class
        {
            if (obj != null)
                action(obj);

            return obj;
        }
    }
}
