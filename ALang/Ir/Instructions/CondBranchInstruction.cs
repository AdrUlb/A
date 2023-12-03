internal sealed class CondBranchInstruction(int sourceReg) : IrInstruction
{
	public int TrueBranch = -1;
	public int FalseBranch = -1;
	public readonly int SourceReg = sourceReg;
}
