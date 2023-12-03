internal sealed class CallFunctionByNameInstruction(string name, int[] argRegIndices, int destReg) : IrInstruction
{
	public readonly string Name = name;
	public readonly int[] ArgRegIndices = argRegIndices;
	public readonly int DestReg = destReg;
}
