using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace auth_server.Endpoints
{
    public static class EIDModule
    {
        static ConcurrentDictionary<string, byte[]> challengeStore = new ConcurrentDictionary<string, byte[]>();

        public static void EIDAuthenticateModule(this IEndpointRouteBuilder app) 
        {
            app.MapPost("/auth/authenticate", async context =>
            {
                var request = await context.Request.ReadFromJsonAsync<AuthRequest>();
                if (request == null)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Invalid request.");
                    return;
                }


      

                // Retrieve the saved challenge for the client
                if (!challengeStore.TryGetValue(request.ClientId, out var savedChallenge))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Invalid request: No challenge found for this client.");
                    return;
                }
           
                // Verify the signature
                var isAuthenticated = Authenticate(savedChallenge, request.Signature);
                if (isAuthenticated)
                {
                    // Clear the challenge after successful authentication
                    challengeStore.TryRemove(request.ClientId, out _);
                }

                await context.Response.WriteAsJsonAsync(new { authenticated = isAuthenticated });
            });


            app.MapGet("/auth/challenge", async context =>
            {
                // Check if clientId is provided in the query string
                if (!context.Request.Query.TryGetValue("clientId", out var clientIdValues) ||
                    string.IsNullOrEmpty(clientIdValues.FirstOrDefault()))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new { error = "ClientId is required." });
                    return;
                }

                string clientId = clientIdValues.First()!;
                var challenge = GenerateChallenge();

                // Save the challenge in memory (or a database)
                challengeStore[clientId] =challenge;

                await context.Response.WriteAsJsonAsync(new { challenge =challenge });
            });

        }

        private static byte[] GenerateChallenge()
        {
            return Guid.NewGuid().ToByteArray();
        }



        private static bool Authenticate(byte[] challenge, byte[] signature)
        {
            try
            {

        
                var publicKeyBytes = GetPublicKey(); // Retrieve the client's public key
                PublicKey pubkey=PublicKey.CreateFromSubjectPublicKeyInfo(publicKeyBytes, out _);
                var parameters=getECParameters(pubkey);
                ECDsa key = ECDsa.Create(parameters);
                return key.VerifyData(challenge, signature, HashAlgorithmName.SHA384);


            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                return false;
            }
        }

        private static byte[] GetPublicKey()
        {
            // Replace this with your logic to get the public key from the eID card
            // For example, you might read it from a certificate or another source
            return Convert.FromBase64String("MHYwEAYHKoZIzj0CAQYFK4EEACIDYgAExa/dAr+YHm1K9ytDopdcMVAehzWuYyzpyek7Aei85DrLr54hP9W63BiB3HeKhXc7HK78X3v6y+cWAMLNwW2MGmyM7dppzadf5q1qb3K5AodzgnRzQteWAdw7zqFoUKvQ");
        }




        private static ECParameters getECParameters(PublicKey publicKey)
        {

            // Offset(dec)       ENCODING            ASN.1 Syntax
            //  00               06 05                -- OBJECT_ID LENGTH
            //  02               2B 81 04 00 22      Secp384r1(1 3 132 0 34)
            byte[] EncodedParamsCurve = publicKey.EncodedParameters.RawData;

            // Offset(dec)       ENCODING            ASN.1 Syntax
            //  00              04                  compression byte
            //  01              { 48 bytes}          --X coordinate
            //  49:             { 48 bytes}          --Y coordinate
            byte[] EncodedParamsPoint = publicKey.EncodedKeyValue.RawData;

            byte[] KeyParams = new byte[5];
            byte[] Secp384r1 = { 0x2B, 0x81, 0x04, 0x00, 0x22 };

            byte[] KeyValue_X = new byte[48];
            byte[] KeyValue_Y = new byte[48];

            Array.Copy(EncodedParamsCurve, 0x02, KeyParams, 0, 5);

            ECParameters parameters = new ECParameters();


            //check if the curve is Secp384r1(1 3 132 0 34)
            if (System.Collections.StructuralComparisons.StructuralEqualityComparer.Equals(KeyParams, Secp384r1))
            {
                //Fill in parameters named curve:
                //Create a named curve using the specified Oid object.
                System.Security.Cryptography.Oid cardP384oid = new Oid("ECDSA_P384");
                parameters.Curve = ECCurve.CreateFromOid(cardP384oid);

                Array.Copy(EncodedParamsPoint, 0x01, KeyValue_X, 0, 48);
                Array.Copy(EncodedParamsPoint, 0x31, KeyValue_Y, 0, 48);

                //Fill in parameters public key (Q)
                System.Security.Cryptography.ECPoint Q;
                Q.X = KeyValue_X;
                Q.Y = KeyValue_Y;

                parameters.Q = Q;
            }
            else
            {
                //not supported, cannot verify, exit
                return parameters;
            };




            return parameters;



        }


    }

    public class AuthRequest
    {
        public string ClientId { get; set; } // Unique identifier for the client
         // Challenge sent by the server (optional)
        public byte[] Signature { get; set; } // Signature from the client (optional)
    }

}

