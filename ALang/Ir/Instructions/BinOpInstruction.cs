internal abstract class BinOpInstruction(int leftSourceReg, int rightSourceReg, int destReg) : IrInstruction
{
	public readonly int LeftSourceReg = leftSourceReg;
	public readonly int RightSourceReg = rightSourceReg;
	public readonly int DestReg = destReg;
}
