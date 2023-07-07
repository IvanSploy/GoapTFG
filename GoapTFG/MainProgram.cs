namespace GoapTFG
{
    internal static class MainProgram
    {
        public static int Main(string[] args)
        {
            int selection = -1;
            if (args.Length != 0)
            {
                selection = int.Parse(args[0]);
            }
            
            switch (selection)
            {
                default:
                    ProgramHanoi.Execute(5,3);
                    break;
                case 1:
                    ProgramHanoi.Execute(4,3);
                    break;
            }

            return 0;
        }
    }
}