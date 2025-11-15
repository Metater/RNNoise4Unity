using System;

namespace Adrenak.RNNoise4Unity {
    public class Denoiser : IDisposable {
        int frameSize;
        IntPtr state;

        float[] processingBuffer;
        int processingBufferDataStart;

        float[] processedData;
        int processedDataRemaining;

        public Denoiser(int frameSize = 480) {
            this.frameSize = frameSize;
            state = Native.rnnoise_create(IntPtr.Zero);

            processingBuffer = new float[frameSize];
            processedData = new float[frameSize];
        }

        public unsafe int Denoise(Span<float> buffer, bool finish = false) {
            int count = 0;

            fixed (float* processingPtr = &processingBuffer[0])
            fixed (float* bufferPtr = buffer) {
                while (buffer.Length > 0 || processingBufferDataStart == frameSize) {
                    if (processedDataRemaining > 0) {
                        // copy new data to the processing buffer
                        var sourceSlice = buffer;

                        if (sourceSlice.Length > processedDataRemaining) {
                            sourceSlice = sourceSlice.Slice(0, processedDataRemaining);
                        }

                        sourceSlice.CopyTo(processingBuffer.AsSpan().Slice(processingBufferDataStart));
                        processingBufferDataStart += sourceSlice.Length;

                        var processed = processedData.AsSpan().Slice(processedData.Length - processedDataRemaining);

                        if (processed.Length > buffer.Length) {
                            processed = processed.Slice(0, buffer.Length);
                        }

                        processed.CopyTo(buffer);

                        buffer = buffer.Slice(processed.Length);

                        processedDataRemaining -= processed.Length;
                        count += processed.Length;
                    }

                    if (processingBufferDataStart > 0 || buffer.Length < frameSize) {
                        // needs to use the processing buffer for this frame
                        var processing = processingBuffer.AsSpan();
                        processing = processing.Slice(processingBufferDataStart);

                        var sourceSlice = buffer;

                        if (sourceSlice.Length > processing.Length) {
                            sourceSlice = sourceSlice.Slice(0, processing.Length);
                        }

                        sourceSlice.CopyTo(processing);

                        processingBufferDataStart += sourceSlice.Length;

                        processing = processing.Slice(sourceSlice.Length);

                        if (processing.Length == 0 || finish) {
                            if (processing.Length > 0)
                                processing.Fill(0);

                            for (int i = 0; i < frameSize; i++)
                                processingBuffer[i] *= Native.SIGNAL_SCALE;

                            fixed (float* processedPtr = &processedData[0])
                                Native.rnnoise_process_frame(state, processedPtr, processingPtr);

                            for (int i = 0; i < frameSize; i++)
                                processedData[i] *= Native.SIGNAL_SCALE_INV;

                            processedDataRemaining = frameSize;

                            var processed = processedData.AsSpan();

                            if (processed.Length > sourceSlice.Length)
                                processed = processed.Slice(0, sourceSlice.Length);

                            processed.CopyTo(buffer);

                            count += sourceSlice.Length;

                            if (finish)
                                processedDataRemaining = 0;
                            else
                                processedDataRemaining -= processed.Length;

                            processingBufferDataStart = 0;
                        }

                        buffer = buffer.Slice(sourceSlice.Length);
                    }
                    else {
                        // can process the source buffer directly without extra copies
                        for (int i = 0; i < frameSize; i++)
                            buffer[i] *= Native.SIGNAL_SCALE;

                        Native.rnnoise_process_frame(state, bufferPtr + count, bufferPtr + count);

                        for (int i = 0; i < frameSize; i++)
                            buffer[i] *= Native.SIGNAL_SCALE_INV;

                        buffer = buffer.Slice(frameSize);

                        count += frameSize;
                    }
                }
            }

            return count;
        }

        public void Dispose() {
            if (state != IntPtr.Zero) {
                Native.rnnoise_destroy(state);
                state = IntPtr.Zero;
            }

            processingBuffer = null;
        }
    }
}
