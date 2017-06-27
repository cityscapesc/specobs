// FftwInterop.h

#pragma once

#include "fftw3.h"

using namespace System;
using namespace System::Diagnostics;
using namespace System::Numerics;

#define fftwPlanDft1d fftw_plan_dft_1d

namespace FftwInterop {

	public ref class Fftw
	{
        private:
            fftw_plan plan1d;
            //fftw_plan planWnd;

        public:
            Fftw()
            {
                plan1d = NULL;
                //planWnd = NULL;
            }

            void BuildPlan1d(int dataLength)
            {
                // From the help file...
                // in and out point to the input and output arrays of the transform, which may be the
                // same (yielding an in-place transform). These arrays are overwritten during planning,
                // unless FFTW_ESTIMATE is used in the flags. (The arrays need not be initialized, but
                // they must be allocated.)

                array<Complex>^ input = gcnew array<Complex>(dataLength);
                pin_ptr<Complex> mp = &input[0];
                fftw_complex* np = reinterpret_cast<fftw_complex*>(mp);

                plan1d = fftwPlanDft1d(dataLength, np, np, FFTW_FORWARD, FFTW_MEASURE | FFTW_UNALIGNED);
            }

            void Execute1d(array<Complex>^ data)
            {
                Debug::Assert(plan1d != NULL);

                pin_ptr<Complex> mp = &data[0];
                fftw_complex* np = reinterpret_cast<fftw_complex*>(mp);

                // Allows the reuse of a plan with different buffers than were used to create the plan
                fftw_execute_dft(plan1d, np, np);
            }

            //void BuildPlanWindow(int samplesPerWindow, int numberOfWindows)
            //{
            //    // From the help file...
            //    // in and out point to the input and output arrays of the transform, which may be the
            //    // same (yielding an in-place transform). These arrays are overwritten during planning,
            //    // unless FFTW_ESTIMATE is used in the flags. (The arrays need not be initialized, but
            //    // they must be allocated.)
            //    array<Complex>^ input = gcnew array<Complex>(samplesPerWindow * numberOfWindows);
            //    pin_ptr<Complex> mpInOut = &input[0];
            //    fftw_complex* npInOut = reinterpret_cast<fftw_complex*>(mpInOut);

            //    int rank = 1;
            //    const int* n = &samplesPerWindow;
            //    int howmany = numberOfWindows;
            //    const int* nembed = n;
            //    int stride = 1; // Contiguous in memory
            //    int dist = samplesPerWindow;

            //    planWnd = fftw_plan_many_dft(rank, n, howmany, npInOut, nembed, stride, dist, npInOut, nembed, stride, dist, FFTW_FORWARD, FFTW_MEASURE | FFTW_UNALIGNED);
            //}

            //void ExecuteWindow(array<Complex>^ data)
            //{
            //    Debug::Assert(planWnd != NULL);

            //    pin_ptr<Complex> mp = &data[0];
            //    fftw_complex* np = reinterpret_cast<fftw_complex*>(mp);

            //    // Allows the reuse of a plan with different buffers than were used to create the plan
            //    fftw_execute_dft(planWnd, np, np);
            //}
	};
}
