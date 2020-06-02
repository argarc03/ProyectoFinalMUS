s.boot

// TEST

play{SinOsc.ar(OnePole.ar(Mix(
	LFSaw.ar([1,0.99],[0,0.6],2000,2000).trunc([400,600])*[1,-1]
),0.98)).dup*0.1}

// SAMPLERS DEFINITIONS----

// Sin
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

// Square
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

// OSC DEFINITIONS-----

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

var objects = Array.newClear(indexedSize: 50);

// Sin

OSCdef.new(
	\sin,
	{arg msg;
		[msg].postln;
		if ( [msg][0][1] == -1.0)
		{
			objects[[msg][0][2]].free();
			objects[[msg][0][2]] = nil;
		}
		{
			objects[[msg][0][2]] = Synth(\sin)
		}
	},
	'/sin',
	nil, 57120
);

// Square

OSCdef.new(
	\square,
	{arg msg;
		[msg].postln;

		if ( [msg][0][1] == -1.0)
		{
			objects[[msg][0][2]].free();
			objects[[msg][0][2]] = nil;
		}
		{
			objects[[msg][0][2]] = Synth(\square)
		}
	},
	'/square',
	nil, 57120
);

// Move sound to new index

OSCdef.new(
	\move,
	{arg msg;
		[msg].postln;
		objects[[msg][0][2]] = objects[[msg][0][1]];

	},
	'/move',
	nil, 57120
);