#!/usr/bin/env python2
# -*- coding: utf-8 -*-
##################################################
# USRP Filter Tester
# Kyeong Su Shin
# Jan 6 2017
##################################################

#import
from gnuradio import eng_notation
from gnuradio import gr
from gnuradio import blocks
from gnuradio import uhd
from gnuradio.eng_option import eng_option
from gnuradio.fft import logpwrfft
from gnuradio.filter import firdes
from optparse import OptionParser
import sip
import sys
import ctypes
import time

from FFTshift import FFTshift
from vector_slice import vector_slice

#top block
class top_block(gr.top_block):

	#main
	def __init__(self):
		gr.top_block.__init__(self, "Top Block")

		##################################################
		# Variables
		##################################################
		self.freq_M = freq_M = 1128
		self.samp_rate = samp_rate = 25e6
		self.freq = freq = freq_M*1e6
		self.bb_gain = bb_gain = 5

		##################################################
		# Blocks
		##################################################
				
		#USRP Source
		self.uhd_usrp_source_0 = uhd.usrp_source(
			",".join(("", "")),
			uhd.stream_args(
				cpu_format="fc32",
				channels=range(1),
			),
		)
		self.uhd_usrp_source_0.set_samp_rate(samp_rate)
		self.uhd_usrp_source_0.set_center_freq(freq, 0)
		self.uhd_usrp_source_0.set_gain(bb_gain, 0)
		self.uhd_usrp_source_0.set_antenna("RX2", 0)
		self.uhd_usrp_source_0.set_bandwidth(samp_rate, 0)
		
		#FFT
		self.logpwrfft_x_0 = logpwrfft.logpwrfft_c(
			sample_rate=samp_rate,
			fft_size=1024,
			ref_scale=2,
			frame_rate=10,
			avg_alpha=0.01,
			average=True,
		)
		
		#FFT Shift
		self.fft_shift = FFTshift(1024,True)
		
		#Cherrypick target freq
		self.slicer = vector_slice(1024,64,550)
				
		#sink
		self.blocks_null_sink_0 = blocks.null_sink(gr.sizeof_float*64)

		##################################################
		# Connections
		##################################################
		self.connect(self.uhd_usrp_source_0, self.logpwrfft_x_0, self.fft_shift , self.slicer ,self.blocks_null_sink_0)
		

	def set_samp_M(self, samp_M):
		self.set_samp_rate(samp_M*1e6)

	def get_freq_M(self):
		return self.freq_M

	def set_freq_M(self, freq_M):
		self.freq_M = freq_M
		self.set_freq(self.freq_M*1e6)

	def get_samp_rate(self):
		return self.samp_rate

	def set_samp_rate(self, samp_rate):
		self.samp_rate = samp_rate
		self.logpwrfft_x_0.set_sample_rate(self.samp_rate)
		self.uhd_usrp_source_0.set_samp_rate(self.samp_rate)
		self.uhd_usrp_source_0.set_bandwidth(self.samp_rate, 0)

	def get_freq(self):
		return self.freq

	def set_freq(self, freq):
		self.freq = freq
		self.uhd_usrp_source_0.set_center_freq(self.freq, 0)

	def get_bb_gain(self):
		return self.bb_gain

	def set_bb_gain(self, bb_gain):
		self.bb_gain = bb_gain
		self.uhd_usrp_source_0.set_gain(self.bb_gain, 0)
		
	def get_max(self):
		return self.slicer.get_max()

	def get_min(self):
		return self.slicer.get_min()
		
	def set_slicer_position(self,val):
		self.slicer.update_start(val)
			
#control logic
class mainProcess(object):
	#constructor
	def __init__(self,tb,samp_rate,start_freq,end_freq,input_sig_freq,step):
		self.start_freq = start_freq
		self.end_freq = end_freq
		self.freq = start_freq
		self.input_sig_freq = input_sig_freq
		self.step = step
		self.tb = tb
		self.terminate = False
		self.samp_rate = samp_rate
		self.tb.set_samp_rate(samp_rate)
		
	def set_top_block(self, tb):
		self.tb = tb

	#main loop
	def main_loop(self):
		print "centre_freq,amplitude" 		
		while self.freq <= self.end_freq:
			self.tb.set_freq(self.freq)
			self.tb.set_slicer_position(self.calc_slicer_pos(self.freq))
			time.sleep(5)
			print str(self.freq)+","+str(self.tb.get_max() - self.tb.get_min())
			self.freq = self.freq + self.step
	
	def calc_slicer_pos(self,freq):
		pos = round((self.input_sig_freq - (freq - (self.samp_rate)/2)) / (self.samp_rate / 1024))
		pos = pos % 1024
		if (pos + 64 >= 1024):
			pos = 959	
		if (pos < 0):
			pos = 0		
		#print pos
		return pos

#main()
def main(top_block_cls=top_block, options=None):

	tb = top_block_cls()
	tb.start()
	ps = mainProcess(tb,12.5e6,1115e6,1145e6,1130e6,5e4)
#	ps = mainProcess(tb,12.5e6,1115e6,1145e6,1130e6,2.5e5)
	ps.main_loop()

	#def quitting():
	tb.stop()
	tb.wait()

#Actual entry
if __name__ == '__main__':
	main()
