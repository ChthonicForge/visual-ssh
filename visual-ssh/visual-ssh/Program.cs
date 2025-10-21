namespace visualSSH;

class Program
{

    static void Main()
    {
        try
        {
            var app = new App();
            app.Run();
        }
        catch (Exception ex)
        {
            throw ex;
        }

    }
}
