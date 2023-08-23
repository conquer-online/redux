using System;
using System.Runtime.InteropServices;
using OpenSSL.Crypto;
using OpenSSL.Crypto.EC;


namespace Redux.Cryptography
{
    public class GameCryptography
    {
        CipherContext _blowfish;
        byte[] _key = new byte[128];
        byte[] _encryptIV = new byte[8];
        byte[] _decryptIV = new byte[8];

        public GameCryptography(byte[] key)
        {
            _blowfish = new CipherContext(Cipher.Blowfish_CFB64);
            Buffer.BlockCopy(key, 0, _key, 0, key.Length);
        }

        public void Decrypt(byte[] packet)
        {
            byte[] buffer = _blowfish.Decrypt(packet, _key, _decryptIV);
            System.Buffer.BlockCopy(buffer, 0, packet, 0, buffer.Length);
        }

        public void Encrypt(byte[] packet)
        {
            byte[] buffer = _blowfish.Encrypt(packet, _key, _encryptIV);
            System.Buffer.BlockCopy(buffer, 0, packet, 0, buffer.Length);
        }

        public CipherContext Blowfish
        {
            get { return _blowfish; }
        }
        public void SetKey(byte[] key)
        {
            Buffer.BlockCopy(key, 0, _key, 0, key.Length);
        }
        public void SetIvs(byte[] i1, byte[] i2)
        {
            Buffer.BlockCopy(i1, 0, _encryptIV, 0, i1.Length);
            Buffer.BlockCopy(i2, 0, _decryptIV, 0, i2.Length);
        }
    }

}
