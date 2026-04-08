using System;

namespace SsmsAutocompletion {

    internal sealed class ConnectionKey {
        private readonly string _value;

        public ConnectionKey(string value) {
            _value = (value ?? "").ToUpperInvariant();
        }

        public bool IsEmpty => string.IsNullOrEmpty(_value);

        public override string ToString() => _value;

        public override bool Equals(object obj) {
            if (obj is ConnectionKey other)
                return string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);
            return false;
        }

        public override int GetHashCode() =>
            StringComparer.OrdinalIgnoreCase.GetHashCode(_value ?? "");
    }
}
