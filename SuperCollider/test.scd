s.boot

// TEST

play{SinOsc.ar(OnePole.ar(Mix(
	LFSaw.ar([1,0.99],[0,0.6],2000,2000).trunc([400,600])*[1,-1]
),0.98)).dup*0.1}

// SAMPLERS DEFINITIONS

//~foodVar = Buffer.read(Server.default);

SynthDef(\sampler,
	{
		arg out = 0,
		rate = 1,
		amp = 0.5,
		buffer = 0;
		var signal;

		signal = Pluck.ar([SinOsc.ar(440, 0, 0.2), SinOsc.ar(442, 0, 0.2)], Impulse.kr(2));
		/*Pluck.ar(1, buffer,
		BufRateScale.kr(buffer)*rate,
		doneAction:2
		);*/

		Out.ar(out, signal*amp);
}).add;

// OSC DEFINITIONS

// Stop all
OSCdef.new(
	\stopAll,
	{arg msg;
		[msg].postln;

		Server.freeAll();
	},
	'/stopAll',
	nil, 57120
);


// Synth

x = nil;

OSCdef.new(
	\synth,
	{arg msg;
		[msg].postln;

		if ( [msg][0][1] == -1.0)
		{
			x.free();
			x = nil;
		}
		{
			x = Synth(\sampler)
		}
	},
	'/synth',
	nil, 57120
);