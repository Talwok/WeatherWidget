using System.IO;
using System.Xml.Serialization;
public static class ParametersSerializer
{
    private static string filename = "Parameters.xml";
    public static string BaseDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

    public static Parameters Reload(this Parameters parameters)
    {
        parameters.Serialize();
        return Deserialize();
    }
    public static void Serialize(this Parameters parameters)
    {
        XmlSerializer xmlSerializerserializer = new XmlSerializer(typeof(Parameters));
        using (TextWriter writer = new StreamWriter(Path.Combine(BaseDir, filename)))
        {
            xmlSerializerserializer.Serialize(writer, parameters);
        }
    }
    public static Parameters Deserialize()
    {
        Parameters? parameters = new Parameters();
        if (!File.Exists(Path.Combine(BaseDir, filename)))
        {
            parameters.Serialize();
            return parameters;
        }
        XmlSerializer xmlSerializerserializer = new XmlSerializer(typeof(Parameters));
        using (TextReader reader = new StreamReader(Path.Combine(BaseDir, filename)))
        {
            parameters = xmlSerializerserializer.Deserialize(reader) as Parameters;
        }
        return parameters;
    }
}