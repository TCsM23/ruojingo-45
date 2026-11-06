using System.Runtime.CompilerServices;

// The Editor builder wires [SerializeField] internal fields directly (compile-time checked),
// and the EditMode tests can reach internal helpers.
[assembly: InternalsVisibleTo("RuoJingo.Editor")]
[assembly: InternalsVisibleTo("RuoJingo.Tests.EditMode")]
