using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ByteArray 
{
    /// <summary>
    /// Ĭ�ϴ�С
    /// </summary>
    const int DEFAULT_SIZE = 1024;
    /// <summary>
    /// ��ʼ��С
    /// </summary>
    private int initSize;
    /// <summary>
    /// �ֽ�����
    /// </summary>
    public byte[] bytes;
    /// <summary>
    /// ����λ��
    /// </summary>
    public int readIndex;
    /// <summary>
    /// д��λ��
    /// </summary>
    public int writeIndex;
    /// <summary>
    /// ����
    /// </summary>
    private int capacity;
    /// <summary>
    /// ��д֮��ĳ���
    /// </summary>
    public int Length { get { return writeIndex - readIndex; } }
    /// <summary>
    /// ��������
    /// </summary>
    public int Remain { get { return capacity-writeIndex; } }
    /// <summary>
    /// �ṩ���ȵĹ��캯��
    /// </summary>
    /// <param name="size">���鳤��</param>
    public ByteArray(int size = DEFAULT_SIZE)
    {
        bytes=new byte[size];
        initSize = size;
        capacity = size;
        readIndex = 0;
        writeIndex = 0;
    }
    /// <summary>
    /// �ṩ�ֽ�����Ĺ��캯��
    /// </summary>
    /// <param name="defaultBytes">�ֽ�����</param>
    public ByteArray(byte[] defaultBytes)
    {
        bytes = defaultBytes;
        initSize = defaultBytes.Length;
        capacity = defaultBytes.Length;
        readIndex=0;
        writeIndex=defaultBytes.Length;
    }
    /// <summary>
    /// �ƶ�����
    /// </summary>
    public void MoveBytes()
    {
        if (Length > 0)
        {
            Array.Copy(bytes, readIndex, bytes, 0, Length);
        }
        writeIndex = Length;
        readIndex = 0;
    }
    /// <summary>
    /// ����ߴ�
    /// </summary>
    /// <param name="size">�µĳ���</param>
    /// <summary>
    /// ����ߴ�
    /// </summary>
    /// <param name="size">�µĳ���</param>
    public void ReSize(int size)
    {
        if (size < Length)
            return;
        if (size < initSize)
            size = initSize;

        int newCapacity = capacity;
        while (newCapacity < size)
            newCapacity *= 2;

        capacity = newCapacity;
        byte[] newBytes = new byte[capacity];
        Array.Copy(bytes, readIndex, newBytes, 0, Length);
        bytes = newBytes;
        writeIndex = Length;
        readIndex = 0;
    }

    /// <summary>
    /// д���ֽ�����
    /// </summary>
    /// <param name="bs">Ҫд����ֽ�����</param>
    public void WriteBytes(byte[] bs)
    {
        if (Remain < bs.Length)
            ReSize(writeIndex + bs.Length);

        Array.Copy(bs, 0, bytes, writeIndex, bs.Length);
        writeIndex += bs.Length;
    }
}
