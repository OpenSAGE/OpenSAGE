namespace OpenSage.FileFormats.W3d;

public abstract record W3dAnimationChannelBase(W3dChunkType ChunkType) : W3dChunk(ChunkType);
