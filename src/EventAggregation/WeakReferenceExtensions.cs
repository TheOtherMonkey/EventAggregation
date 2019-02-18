using System;

namespace EventAggregation
{
    /// <summary>
    /// Extension methods for the <see cref="WeakReference"/> class.
    /// </summary>
    internal static class WeakReferenceExtensions
    {
        /// <summary>
        /// Determine if the <paramref name="target"/> is the same objects as the <see cref="WeakReference.Target"/> 
        /// of the <paramref name="weakReference"/>.
        /// </summary>
        /// <param name="weakReference">The <see cref="WeakReference"/> to the object being tested.</param>
        /// <param name="target">The object being compared.</param>
        /// <returns>True if the <see cref="WeakReference.Target"/> of <paramref name="weakReference"/> is the same
        /// object as <paramref name="target"/>, otherwise False.</returns>
        public static bool ReferenceEquals(this WeakReference weakReference, object target)
        {
            var result = ReferenceEquals(weakReference.Target, target);
            return result;
        }
    }
}
