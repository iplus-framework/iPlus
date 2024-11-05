using gip.core.layoutengine;
using gip.core.reporthandlerwpf;
using gip.core.visualcontrols;
using gip.core.scichart;

namespace gip.tool.generator;

internal class Program
{
    static void Main()
    {
        Console.WriteLine("Generator for XAML code completion Xsd schema:"+
                          "\n 1. Generate for gip.core.layoutengine"+
                          "\n 2. Generate for gip.core.report handler"+
                          "\n 3. Generate for gip.core.visualcontrols"+
                          "\n 4. Generate for gip.core.scichart");
        string? selection = Console.ReadLine();


        if (int.TryParse(selection, out int mode))
        {
            switch (mode)
            {
                case 1:
                        CodeCompletionXsdGeneratorVB.RunTool();
                    break;
                case 2:
                        CodeCompletionXsdGeneratorVBR.RunTool();
                    break;
                case 3:
                        CodeCompletionXsdGeneratorVBV.RunTool();
                    break;
                case 4:
                        CodeCompletionXsdGeneratorVBC.RunTool();
                    break;
            }
        }
    }
}


