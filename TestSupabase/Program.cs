using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        string supabaseUrl = "https://uslbuyaouymccltdwbmm.supabase.co";
        string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InVzbGJ1eWFvdXltY2NsdGR3Ym1tIiwicm9sZSI6ImFub24iLCJpYXQiOjE3Nzg3NzI0MDEsImV4cCI6MjA5NDM0ODQwMX0.dKFU7ePEOF3MBsTPSKSnyJiDLCT-gBX2S7bK50E5-J4";

        var client = new Supabase.Client(supabaseUrl, supabaseKey, new Supabase.SupabaseOptions { AutoRefreshToken = false, AutoConnectRealtime = false });
        
        
        try
        {
            await client.Auth.SignOut();
            Console.WriteLine("Login successful: " + session?.User?.Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception: " + ex.ToString());
        }
    }
}
