internal sealed class CallFunctionInstruction(int funcIndex, int[] argRegIndices, int destReg) : IrInstruction
{
	public readonly int FuncIndex = funcIndex;
	public readonly int[] ArgRegIndices = argRegIndices;
	public readonly int DestReg = destReg;
}
