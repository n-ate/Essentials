using n_ate.Essentials;
using n_ate.Essentials.Enumerations;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace n_ate.Essentials.Models
{
    public class TrackingBase
    {
        private readonly Dictionary<string, object?> _initialPropertyValues = new Dictionary<string, object?>();
        private readonly Dictionary<string, object?> _setProperties = new Dictionary<string, object?>();

        private readonly object _lock = new object();
        private static readonly List<int> _propertyResetAnticascadeThreadId = new List<int>();

        public TrackingBase()
        {
            ResetChangeTracking(true);
        }

        /// <summary>
        /// Gets properties that have been set using their setters even if the property value is unchanged.
        /// </summary>
        /// <param name="capitalization">Forces the character case of the dictionary keys (property names) into a particular capitalization scheme.</param>
        /// <returns>The property-name-value dictionary of set properties.</returns>
        public Dictionary<string, object?> GetPropertiesThatHaveBeenSet(Capitalization capitalization = Capitalization.None)
        {
            return this._setProperties.ToDictionary(capitalization);
        }

        /// <summary>
        /// Marks all properties as having been set by their setters.
        /// </summary>
        public void MarkAllPropertiesAsSet()
        {
            foreach (var kv in this._initialPropertyValues)
            {
                if (!this._setProperties.ContainsKey(kv.Key)) this._setProperties[kv.Key] = kv.Value;
            }
        }

        /// <summary>
        /// Gets properties that have [NOT] been set using their setters even if the property value is unchanged.
        /// </summary>
        /// <param name="capitalization">Forces the character case of the dictionary keys (property names) into a particular capitalization scheme.</param>
        /// <returns>The property-name-value dictionary of non-set properties.</returns>
        public Dictionary<string, object?> GetPropertiesThatHaveNotBeenSet(Capitalization capitalization = Capitalization.None)
        {
            var initialProperties = this._initialPropertyValues.ToDictionary();
            foreach (var property in this._setProperties) initialProperties.Remove(property.Key);
            return initialProperties.ToDictionary(capitalization);
        }

        /// <summary>
        /// Gets properties with current values that differ from their initial values.
        /// </summary>
        /// <param name="capitalization">Forces the character case of the dictionary keys (property names) into a particular capitalization scheme.</param>
        /// <returns>The property-name-value dictionary of properties with changed values.</returns>
        public Dictionary<string, object?> GetPropertiesWithChangedValues(Capitalization capitalization = Capitalization.None)
        {
            var current = this.ToDictionary(capitalization);
            var initial = this._initialPropertyValues.ToDictionary(capitalization);
            foreach (var kv in initial)
            { //remove unchanged values from current to make it a changed values dictionary
                if (current[kv.Key]!.Equals(kv.Value)) current.Remove(kv.Key);
            }
            return current;
        }

        /// <summary>
        /// Gets all properties as a dictionary with their current values.
        /// </summary>
        /// <param name="capitalization">Forces the character case of the dictionary keys (property names) into a particular capitalization scheme.</param>
        /// <returns>The property-name-value dictionary.</returns>
        public Dictionary<string, object?> GetAllProperties(Capitalization capitalization = Capitalization.None)
        {
            var current = this.ToDictionary(capitalization);
            return current;
        }

        /// <summary>
        /// Checks if the property has been set. This will always return false if tracking is not setup using the <see cref="PropertyChanging(object, string)" /> method.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>True if the property was previously set.</returns>
        public bool HasPropertyBeenSet(string propertyName)
        {
            return _setProperties.ContainsKey(propertyName);
        }

        /// <summary>
        /// Notifies tracking that a property has changed. It is recommended that this method is called from each property setter that requires tracking.
        /// </summary>
        /// <param name="newValue">The new value of the property.</param>
        /// <param name="name">The name of the property.</param>
        public void PropertyChanging(object newValue, [CallerMemberName] string name = "")
        {
            if (_propertyResetAnticascadeThreadId.Contains(Thread.CurrentThread.ManagedThreadId))
            {
                lock (_lock)
                {
                    if (_propertyResetAnticascadeThreadId.Contains(Thread.CurrentThread.ManagedThreadId)) return; //short-circuit -- do not mark property as set because this is a cascading change caused by a reset
                }
            }
            _setProperties[name] = newValue;
        }

        /// <summary>
        /// Resets both change-tracking and set-tracking of properties. This makes the object appear as if it was just initialized with all properties having their current values.
        /// </summary>
        public void ResetAllTracking()
        {
            ResetChangeTracking(true);
            ResetSetPropertiesTracking();
        }

        /// <summary>
        /// Resets the initial values of the object to the current values
        /// </summary>
        /// <param name="returnNull">Do not calculate and return changes made prior to this call. Instead return null. </param>
        /// <returns>All changes made prior to this method call, or null.</returns>
        public Dictionary<string, object?>? ResetChangeTracking(bool returnNull = false)
        {
            var current = this.ToDictionary();
            var changes = returnNull ? null : current.Where(kv => this._initialPropertyValues[kv.Key] != kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value); //returns changed values
            this._initialPropertyValues.Clear();
            foreach (var kv in current) this._initialPropertyValues.Add(kv.Key, kv.Value);
            return changes;
        }

        /// <summary>
        /// Resets a property value and property set-tracking
        /// </summary>
        /// <param name="propertyName">The name of the property to restore to its initial value.</param>
        /// <returns>True if any set-tracking was reset.</returns>
        public bool ResetProperty(string propertyName)
        {
            if (_setProperties.ContainsKey(propertyName))
            {
                lock (_lock) _propertyResetAnticascadeThreadId.Add(Thread.CurrentThread.ManagedThreadId);
                this.SetValue(propertyName, _initialPropertyValues[propertyName]); // set the value to the initial value..
                lock (_lock) _propertyResetAnticascadeThreadId.Remove(Thread.CurrentThread.ManagedThreadId);
                _setProperties.Remove(propertyName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Resets the set-tracking of a single property so that set-tracking shows the property has not been set.
        /// </summary>
        /// <param name="propertyName">The name of the property to reset set-tracking on.</param>
        /// <returns>True if there was a tracked set of the property to reset. False if no property set was tracked.</returns>
        public bool ResetPropertyTrackingOnly(string propertyName)
        {
            return _setProperties.Remove(propertyName);
        }

        /// <summary>
        /// Resets change-tracking for "set" properties so that the current set-tracking shows no properties have been set (does not reset change-tracking; use <see cref="ResetChangeTracking(bool)"/>).
        /// </summary>
        /// <returns>The list of set properties that were tracked prior to the reset.</returns>
        public Dictionary<string, object?> ResetSetPropertiesTracking()
        {
            var result = GetPropertiesThatHaveBeenSet();
            this._setProperties.Clear();
            return result;
        }
    }
}