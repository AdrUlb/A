internal sealed class PhiInstruction(int branch1, int reg1, int branch2, int reg2, int destReg) : IrInstruction
{
	public int Branch1 = branch1;
	public int Reg1 = reg1;
	public int Branch2 = branch2;
	public int Reg2 = reg2;
	public int DestReg = destReg;
}
