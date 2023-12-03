internal sealed class IntegerIrType : IrType
{
	public bool Signed = true;
	public int Bits = -1;

	public IntegerIrType()
	{

	}

	public IntegerIrType(bool signed, int bits)
	{
		Signed = signed;
		Bits = bits;
	}

	public override string ToString() => $"{(Signed ? 'i' : 'u')}{(Bits >= 0 ? Bits.ToString() : "?")}";
}
