internal sealed class StoreArgumentInstruction(int argumentIndex, int destReg) : IrInstruction
{
	public readonly int ArgumentIndex = argumentIndex;
	public readonly int DestReg = destReg;
}
