﻿using SharpDX;
using SharpDX.Direct3D11;
using System;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Navmesh.Render;

// base class for large buffers
// there are two flavours with slightly different performance characteristics and update methods:
// - 'dynamic' - suitable for buffers that are updated frequenty - once per frame or more often
//   these are created with Dynamic usage + cpu Write access and mapped with WriteDiscard flag
// - 'static' - suitable for buffers that are updated infrequenty
//   these are created with Default usage + no cpu access, builder creates temporary Staging buffer, maps it, then copies over to main resource
public class RenderBuffer : IDisposable
{
    public class Builder : IDisposable
    {
        private RenderContext _ctx;
        private RenderBuffer _buffer;
        private DataStream _stream;
        private Buffer? _staging; // only for non-dynamic

        public int CurElements => _buffer.CurElements;

        internal Builder(RenderContext ctx, RenderBuffer buffer)
        {
            _ctx = ctx;
            _buffer = buffer;
            buffer.CurElements = 0;
            if (buffer.Dynamic)
            {
                ctx.Context.MapSubresource(buffer.Buffer, MapMode.WriteDiscard, MapFlags.None, out _stream);
            }
            else
            {
                _staging = new(ctx.Device, new()
                {
                    SizeInBytes = buffer.ElementSize * buffer.MaxElements,
                    Usage = ResourceUsage.Staging,
                    CpuAccessFlags = CpuAccessFlags.Write,
                });
                ctx.Context.MapSubresource(_staging, MapMode.Write, MapFlags.None, out _stream);
            }
        }

        public void Dispose()
        {
            if (_buffer.Dynamic)
            {
                _ctx.Context.UnmapSubresource(_buffer.Buffer, 0);
            }
            else
            {
                _ctx.Context.UnmapSubresource(_staging!, 0);
                _ctx.Context.CopyResource(_staging!, _buffer.Buffer);
                _staging!.Dispose();
                _staging = null;
            }
        }

        // TODO: reconsider this api
        public DataStream Stream => _stream;
        public void Advance(int count)
        {
            _buffer.CurElements += count;
            if (_buffer.CurElements > _buffer.MaxElements)
                throw new ArgumentOutOfRangeException("Buffer overflow");
        }
    }

    public bool Dynamic { get; init; }
    public int ElementSize { get; init; }
    public int MaxElements { get; init; }
    public int CurElements { get; private set; }
    public Buffer Buffer { get; init; }

    public RenderBuffer(RenderContext ctx, int elementSize, int maxElements, BindFlags bindFlags, bool dynamic)
    {
        dynamic = true; // TODO: figure why it doesn't work as expected..
        Dynamic = dynamic;
        ElementSize = elementSize;
        MaxElements = maxElements;
        Buffer = new(ctx.Device, new()
        {
            SizeInBytes = elementSize * maxElements,
            Usage = dynamic ? ResourceUsage.Dynamic : ResourceUsage.Default,
            BindFlags = bindFlags,
            CpuAccessFlags = dynamic ? CpuAccessFlags.Write : CpuAccessFlags.None,
        });
    }

    public void Dispose()
    {
        Buffer.Dispose();
    }

    public Builder Map(RenderContext ctx) => new Builder(ctx, this);
}
