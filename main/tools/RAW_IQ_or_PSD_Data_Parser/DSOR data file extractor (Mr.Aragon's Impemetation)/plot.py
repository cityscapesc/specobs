#!/usr/bin/env python

import sys
import numpy as np
import os
import matplotlib.pyplot as plt
from operator import add

if (len(sys.argv) <= 1):
                print "Usage : ", sys.argv[0], " target_file"
                sys.exit(1)
#if target file not found, warn user.
elif (os.path.exists(sys.argv[1]) == False):
                print "File not found!"
                sys.exit(2)

#load file
ary = np.fromfile(sys.argv[1])

#interleaved data to complex double.
i = ary[::2]
q = [x*1j for x in ary[1::2]]
iq = map(add, i, q)

#plot I component
plt.plot(np.real(iq))
plt.ylabel('Amplitude')
plt.title('I Component of Data')
plt.show()

#plot Q component
plt.plot(np.imag(iq))
plt.ylabel('Amplitude')
plt.title('Q Component of Data')
plt.show()
