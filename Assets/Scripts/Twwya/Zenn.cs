class Foo
{
    private bool _state;

    internal Foo(bool state)
    {
        _state = state;
    }

    internal void Run()
    {
        if (_state)
        {
            B1();
        }
        else
        {
            B2();
        }
    }
    
    void B1()
    {}
    
    void B2()
    {}
}

class Bar
{
    bool _state;
    private Foo _foo;
    
    internal void Prepare()
    {
        _foo = _state ? new Foo(_state) : new Foo(!_state);
    }

    internal void Run()
    {
        _foo.Run();
    }
}
