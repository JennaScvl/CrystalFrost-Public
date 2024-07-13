using UnityEngine;
using UnityEditor;

public class CSProjPostProcessor : AssetPostprocessor {

    private const string EngineCsproj = "CrystalFrostEngine.csproj";

    public static string OnGeneratedCSProject(string path, string content)
    {
        if (path.EndsWith(EngineCsproj))
        {
            content = ProcessCrystalFrostEngineCsProj(content);
        }
        return content;
    }

    private static string ProcessCrystalFrostEngineCsProj(string content)
    {
        Debug.Log("Post Processing  - " + EngineCsproj);
        
        Debug.Log("Fixing T4 Template References - " + EngineCsproj);

        const string findLogMessagesTt =
            "    <None Include=\"Assets\\CFEngine\\Logging\\LogMessages.tt\" />";

        const string replaceLogMessagesTt =
            "    <None Include=\"Assets\\CFEngine\\Logging\\LogMessages.tt\">\r\n" +
            "        <Generator>TextTemplatingFileGenerator</Generator>\r\n" +
            "        <LastGenOutput>LogMessages.cs</LastGenOutput>\r\n" +
            "    </None>";

        const string findLogMessagesCs =
            "    <Compile Include=\"Assets\\CFEngine\\Logging\\LogMessages.cs\" />";

        const string replaceLogMessagesCs =
            "    <Compile Include=\"Assets\\CFEngine\\Logging\\LogMessages.cs\">\r\n" +
            "        <DependentUpon>LogMessages.tt</DependentUpon>\r\n" +
            "        <AutoGen>True</AutoGen>\r\n" +
            "        <DesignTime>True</DesignTime>\r\n" +
            "    </Compile>\r\n";

        content = content.Replace(findLogMessagesTt, replaceLogMessagesTt);


        content = content.Replace(findLogMessagesCs, replaceLogMessagesCs);

        return content;
    }
}