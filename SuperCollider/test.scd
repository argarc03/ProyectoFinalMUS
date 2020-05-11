// SAMPLERS DEFINITIONS

//~foodVar = Buffer.read(Server.default);

SynthDef(\sampler,
	{
		arg out = 0,
		rate = 1,
		amp = 0.5,
		buffer = 0;
		var signal;

		signal =
		Pluck.ar([SinOsc.ar(440, 0, 0.2), SinOsc.ar(442, 0, 0.2)], Impulse.kr(2),0,0);
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


// Food
OSCdef.new(
	\food,
	{arg msg;
		[msg].postln;

		Synth(\sampler);
	},
	'/food',
	nil, 57120
);

// Move
OSCdef.new(
	\move,
	{arg msg;
		[msg].postln;

	},
	'/move',
	nil, 57120
);