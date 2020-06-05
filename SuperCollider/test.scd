s.boot

// TEST
(
play{SinOsc.ar(OnePole.ar(Mix(
	LFSaw.ar([1,0.99],[0,0.6],2000,2000).trunc([400,600])*[1,-1]
),0.98)).dup*0.1}
)

// DRUM BUFFERS

(
d = Dictionary.new;
d.add(\drums -> PathName("Audio/Drums/").entries.collect({
	arg sf;
	Buffer.read(s, sf.fullPath);
    })
)
)

d[\drums].choose.play;

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

// Drums
(
SynthDef.new(\drumbuf, {
	arg buf = 0, rate = 1, amp = 1;
	var sig;
	sig = PlayBuf.ar(2, buf, BufRateScale.ir(buf) * rate, doneAction:2); // DoneAction 2 = free the synth when done
	sig = sig*amp;
	Out.ar(0, sig);
}).add;
)

(
Pdef(
	\drumRhythm,
	Pbind(
		\instrument, \drumbuf,
		\delta, Prand([0.5],inf),
		\buf, Prand(d[\drums], inf),
		\rate, 1,
		\amp, 0.5,
	);
).play;
)

\drumRhythm.

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
			piano = Synth(\piano, [\freq, [msg][0][3]])
		}
	},
	'/piano',
	nil, 57120
);
)

// Drum Preview
(
var drum = nil;
OSCdef.new(
    \drumPrev,
    {arg msg;
        [msg].postln;

        if ( [msg][0][1] == -1.0)
        {
            drum.free();
            drum = nil;
        }
        {
            drum = Synth(\drumbuf, [\buf, d[\drums][[msg][0][2]]])
        }
    },
    '/drumPrev',
    nil, 57120
);
)

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
)
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
