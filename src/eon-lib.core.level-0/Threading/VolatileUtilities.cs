using System.Runtime.CompilerServices;

using MemoryBarrierApi = System.Threading.Interlocked;

namespace Eon.Threading {

	public static class VolatileUtilities {

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static long Read(ref long location) {
			long result;
			MemoryBarrierApi.MemoryBarrier();
			result = location;
			MemoryBarrierApi.MemoryBarrier();
			return result;
		}

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static void Write(ref long location, long value) {
			location = value;
			MemoryBarrierApi.MemoryBarrier();
		}

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static int Read(ref int location) {
			int result;
			MemoryBarrierApi.MemoryBarrier();
			result = location;
			MemoryBarrierApi.MemoryBarrier();
			return result;
		}

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static void Write(ref int location, int value) {
			location = value;
			MemoryBarrierApi.MemoryBarrier();
		}

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static T Read<T>(ref T location) {
			T result;
			MemoryBarrierApi.MemoryBarrier();
			result = location;
			MemoryBarrierApi.MemoryBarrier();
			return result;
		}

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static T Write<T>(ref T location, T value) {
			location = value;
			MemoryBarrierApi.MemoryBarrier();
			return value;
		}

	}

}