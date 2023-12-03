using System.Numerics;

internal sealed class StoreConstInstruction(BigInteger value, int destReg) : IrInstruction
{
	public readonly BigInteger Value = value;
	public readonly int DestReg = destReg;
}
