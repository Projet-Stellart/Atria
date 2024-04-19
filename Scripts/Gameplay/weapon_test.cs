using Godot;

public partial class weapon_test : weapon
{
    AudioStreamPlayer3D stream1;
    AudioStreamPlayer3D stream2;
    AudioStreamPlayer3D stream3;
    AudioStreamPlayer3D reloadStream;
    public new AnimationPlayer firing;
    public new AnimationPlayer reloading;

    [Export]
    public new PackedScene muzzleFlashScene;

    [Export]
    public new int bullets = 1500;
    [Export]
    public new int bulletPerMag = 25;

    [Export]
    public new float damage = 5;

    public new int currBullets;

    public override void _Ready()
    {
        firing = GetNode<AnimationPlayer>("Gunfire");
        reloading = GetNode<AnimationPlayer>("Reloading");
        stream1 = GetNode<AudioStreamPlayer3D>("GunSound1");
        stream2 = GetNode<AudioStreamPlayer3D>("GunSound2");
        stream3 = GetNode<AudioStreamPlayer3D>("GunSound3");
        reloadStream = GetNode<AudioStreamPlayer3D>("ReloadSound");
        bullets -= bulletPerMag;
        currBullets = bulletPerMag;
    }

    public override void Fire() {
        currBullets--;
        PlaySound();
		firing.Play("Fire");
    }

    public override void PlaySound() {
		if (stream1.Playing) {
			if (stream2.Playing)
				stream3.Play();
			else
				stream2.Play();
		} else
			stream1.Play();
	}

    public override void Reload() {
        reloadStream.Play();
        reloading.Play("reload");
    }

    public override void onReload(StringName anim_name) {
        bullets -= bulletPerMag - currBullets;
        currBullets = bulletPerMag;
    }
}
