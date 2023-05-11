﻿using System;
using System.Security.Cryptography;

namespace Cosmos.Cms.Services
{
    /// <summary>
    /// Random number generator
    /// </summary>
    public class RNGCryptoRandomGenerator
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public RNGCryptoRandomGenerator()
        {
        }

        /// <summary>
        /// Get next random number
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxExclusiveValue"></param>
        /// <returns></returns>
        public int Next(int minValue, int maxExclusiveValue)
        {
            if (minValue >= maxExclusiveValue)
                throw new ArgumentOutOfRangeException("minValue must be lower than maxExclusiveValue");

            long diff = (long)maxExclusiveValue - minValue;
            long upperBound = uint.MaxValue / diff * diff;

            uint ui;
            do
            {
                ui = GetRandomUInt();
            } while (ui >= upperBound);
            return (int)(minValue + (ui % diff));
        }

        private uint GetRandomUInt()
        {
            var randomBytes = GenerateRandomBytes(sizeof(uint));
            return BitConverter.ToUInt32(randomBytes, 0);
        }

        private byte[] GenerateRandomBytes(int bytesNumber)
        {
            byte[] buffer = RandomNumberGenerator.GetBytes(bytesNumber);
            return buffer;
        }
    }
}
