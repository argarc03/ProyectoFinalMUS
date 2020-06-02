s.boot

// TEST

play{SinOsc.ar(OnePole.ar(Mix(
	LFSaw.ar([1,0.99],[0,0.6],2000,2000).trunc([400,600])*[1,-1]
),0.98)).dup*0.1}

// SAMPLERS DEFINITIONS----

// Piano preview

(
SynthDef(\piano,
	{
		arg out = 0,
		freq = 440,
		amp = 0.5,
		buffer = 0;
		var signal;

		signal = [SinOsc.ar(freq, 0, 0.2), SinOsc.ar(freq, 0, 0.2)];

		Out.ar(out, signal*amp);
}).add;
)


// Sin
(
SynthDef(\sin,
	{
		arg out = 0,
		rate = 1,
		amp = 0.5,
		buffer = 0;
		var signal;

		signal = Pluck.ar([SinOsc.ar(440, 0, 0.2), SinOsc.ar(440, 0, 0.2)], Impulse.kr(2));

		Out.ar(out, signal*amp);
}).add;
)

// Square
(
SynthDef(\square,
	{
		arg out = 0,
		rate = 1,
		amp = 0.1,
		buffer = 0;
		var signal;

		signal = Pluck.ar([LFPulse.ar(440, 0, 0.2), LFPulse.ar(440, 0, 0.2)], Impulse.kr(2));

		Out.ar(out, signal*amp);
}).add;
)

// OSC DEFINITIONS-----

// Stop all
(
OSCdef.new(
	\stopAll,
	{arg msg;
		[msg].postln;

		Server.freeAll();
	},
	'/stopAll',
	nil, 57120
);
)


// Piano preview

// Piano
(
var piano = nil;

OSCdef.new(
	\piano,
	{arg msg;
		[msg].postln;

		if ( [msg][0][1] == -1.0)
		{
			piano.free();
			piano = nil;
		}
		{
			piano = Synth(\piano, [\freq, [msg][0][2]])
		}
	},
	'/piano',
	nil, 57120
);
)

// Sin

(
x = nil;

OSCdef.new(
	\sin,
	{arg msg;
		[msg].postln;

		if ( [msg][0][1] == -1.0)
		{
			x.free();
			x = nil;
		}
		{
			x = Synth(\sin)
		}
	},
	'/sin',
	nil, 57120
);
)
// Square
(
y = nil;

OSCdef.new(
	\square,
	{arg msg;
		[msg].postln;

		if ( [msg][0][1] == -1.0)
		{
			y.free();
			y = nil;
		}
		{
			y = Synth(\square)
		}
	},
	'/square',
	nil, 57120
);
)