namespace Loqui.Generation;

public class P3IntNullType : P3IntType
{
    public override string TypeName(bool getter, bool needsCovariance = false) => $"{base.TypeName(getter, needsCovariance)}?";

    public override string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate)
    {
        return $"{(negate ? "!" : null)}object.Equals({accessor.Access}, {rhsAccessor.Access})";
    }
}