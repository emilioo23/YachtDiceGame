using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Xml;

namespace YachtDice.Server.Logic.Passkeys
{
    /// <summary>
    /// Checks the client data JSON that the client sends in every ceremony. It must say which ceremony it is for,
    /// carry the challenge that the server issued and name the origin of the game.
    /// </summary>
    public static class ClientDataValidator
    {
        private const int MaxClientDataSize = 2048;

        /// <summary>
        /// Checks the client data of one ceremony.
        /// </summary>
        /// <param name="clientDataJson">Bytes of the client data JSON.</param>
        /// <param name="expectedType">Type that the ceremony must have.</param>
        /// <param name="expectedChallenge">Challenge that the server issued.</param>
        /// <returns>True if the type, the challenge and the origin are the expected ones.</returns>
        public static bool IsValid(byte[] clientDataJson, string expectedType, byte[] expectedChallenge)
        {
            bool isValid = false;

            if (clientDataJson != null && expectedChallenge != null && clientDataJson.Length <= MaxClientDataSize)
            {
                ClientData clientData = Read(clientDataJson);

                isValid = clientData != null
                    && clientData.Type == expectedType
                    && clientData.Challenge == Base64Url.Encode(expectedChallenge)
                    && clientData.Origin == PasskeyRules.Origin;
            }

            return isValid;
        }

        private static ClientData Read(byte[] clientDataJson)
        {
            ClientData clientData;

            try
            {
                using (var stream = new MemoryStream(clientDataJson))
                {
                    clientData = new DataContractJsonSerializer(typeof(ClientData)).ReadObject(stream) as ClientData;
                }
            }
            catch (SerializationException)
            {
                clientData = null;
            }
            catch (XmlException)
            {
                clientData = null;
            }

            return clientData;
        }

        [DataContract]
        private sealed class ClientData
        {
            [DataMember(Name = "type")]
            public string Type { get; set; }

            [DataMember(Name = "challenge")]
            public string Challenge { get; set; }

            [DataMember(Name = "origin")]
            public string Origin { get; set; }
        }
    }
}
