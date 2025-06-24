using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

public class GifDecoder
{
    public List<Texture2D> Frames { get; private set; }
    public List<float> FrameDelays { get; private set; }

    public GifDecoder(byte[] gifData)
    {
        Frames = new List<Texture2D>();
        FrameDelays = new List<float>();
        Frames = LoadGifFromBytes(gifData);//DecodeGif(gifData);
    }

    private void DecodeGif(byte[] gifData)
    {
        using (MemoryStream ms = new MemoryStream(gifData))
        using (BinaryReader reader = new BinaryReader(ms))
        {
            string header = new string(reader.ReadChars(6));
            if (!header.StartsWith("GIF"))
            {
                Debug.LogError("Invalid GIF file");
                return;
            }


            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            byte[] first100Bytes = reader.ReadBytes(100);
            Debug.Log("GIF Header (first 100 bytes): " + BitConverter.ToString(first100Bytes));

            reader.BaseStream.Seek(6, SeekOrigin.Begin);
            ushort width = reader.ReadUInt16();
            ushort height = reader.ReadUInt16();
            reader.BaseStream.Seek(7, SeekOrigin.Current);

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                byte block = reader.ReadByte();
                Debug.Log($"Block: 0x{block:X2} at position {reader.BaseStream.Position}");

                if (block == 0x2C) // Image Descriptor (프레임 시작)
                {
                    Debug.Log("Found Image Descriptor (0x2C)");
                    ReadImageDescriptor(reader, width, height);
                }
                else if (block == 0x21) // Extension Block (딜레이 포함)
                {
                    Debug.Log("Found Extension Block (0x21)");
                    ReadExtensionBlock(reader);
                }
                else if (block == 0x3B) // GIF 종료
                {
                    Debug.Log("GIF End (0x3B)");
                    break;
                }
                else
                {
                    Debug.Log($"Skipping Unknown Block: 0x{block:X2}");
                }
            }

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                long posBefore = reader.BaseStream.Position;
                byte block = reader.ReadByte();

                Debug.Log("Block: " + block.ToString("X2") + " at position: " + posBefore);
                // ...
                if (block == 0x2C) // Image Descriptor (새 프레임)
                {
                    ReadImageDescriptor(reader, width, height);
                }
                else if (block == 0x21) // Extension Block (딜레이 처리)
                {
                    ReadExtensionBlock(reader);
                }
                else if (block == 0xFF) // 애플리케이션 익스텐션 블록
                {
                    int subBlockSize = reader.ReadByte();
                    byte[] identifier = reader.ReadBytes(subBlockSize);

                    string appIdentifier = Encoding.ASCII.GetString(identifier);
                    Debug.Log("Application Extension Identifier: " + appIdentifier);

                    if (appIdentifier.Contains("NETSCAPE2.0"))
                    {
                        Debug.Log("This is an animated GIF!");
                    }
                }
                else if (block == 0x3B) // GIF 종료
                {
                    break;
                }
            }
        }
    }

    private void ReadImageDescriptor(BinaryReader reader, int width, int height)
    {
        reader.BaseStream.Seek(8, SeekOrigin.Current); // 이미지 오프셋 스킵
        byte packed = reader.ReadByte();
        bool hasLocalColorTable = (packed & 0x80) != 0;
        int colorTableSize = hasLocalColorTable ? 3 * (1 << ((packed & 7) + 1)) : 0;

        reader.BaseStream.Seek(colorTableSize, SeekOrigin.Current);
        reader.BaseStream.Seek(1, SeekOrigin.Current); // LZW Min Code Size

        List<byte> imageData = new List<byte>();
        byte blockSize;
        while ((blockSize = reader.ReadByte()) != 0)
        {
            imageData.AddRange(reader.ReadBytes(blockSize));
        }

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.SetPixels32(FakeDecode(imageData.ToArray(), width, height));
        texture.Apply();

        Frames.Add(texture);
        FrameDelays.Add(0.1f);
    }

    private void ReadExtensionBlock(BinaryReader reader)
    {
        byte label = reader.ReadByte();
        if (label == 0xF9) // Graphic Control Extension
        {
            reader.ReadByte();
            ushort delay = reader.ReadUInt16();
            reader.ReadByte();
            reader.ReadByte();

            if (FrameDelays.Count > 0)
            {
                FrameDelays[FrameDelays.Count - 1] = delay / 100f;
            }
        }
        else
        {
            while (reader.ReadByte() != 0) { }
        }
    }

    private Color32[] FakeDecode(byte[] data, int width, int height)
    {
        Color32[] pixels = new Color32[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color32((byte)(i % 256), (byte)(i % 256), (byte)(i % 256), 255);
        }
        return pixels;
    }

    public static List<Texture2D> LoadGifFromBytes(byte[] gifData)
    {
        List<Texture2D> frames = new List<Texture2D>();

        using (MemoryStream ms = new MemoryStream(gifData))
        using (Image gifImg = Image.FromStream(ms))
        {
            FrameDimension dimension = new FrameDimension(gifImg.FrameDimensionsList[0]);
            int frameCount = gifImg.GetFrameCount(dimension);

            for (int i = 0; i < frameCount; i++)
            {
                gifImg.SelectActiveFrame(dimension, i);
                using (Bitmap frameBmp = new Bitmap(gifImg))
                {
                    Texture2D texture = new Texture2D(frameBmp.Width, frameBmp.Height, TextureFormat.RGBA32, false);
                    for (int y = 0; y < frameBmp.Height; y++)
                    {
                        for (int x = 0; x < frameBmp.Width; x++)
                        {
                            System.Drawing.Color pixelColor = frameBmp.GetPixel(x, frameBmp.Height - 1 - y);
                            texture.SetPixel(x, y, new UnityEngine.Color(pixelColor.R / 255f, pixelColor.G / 255f, pixelColor.B / 255f, pixelColor.A / 255f));
                        }
                    }
                    texture.Apply();
                    frames.Add(texture);
                }
            }
        }

        return frames;
    }
}
