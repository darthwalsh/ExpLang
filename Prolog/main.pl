/* Run with command:   
    swipl -g main -t halt main.pl 12a8 124b
  to solve and find {a:4,b:8}

  Run without arguments to test many permutations
*/

% TODO possible to rewrite with all vars by creating an array like length(X, 4)?

solve(X, Y, Env) :-
    string(X),
    string(Y),
    string_chars(X, XX),
    string_chars(Y, YY),
    solve(XX, YY, Env).

solve([], [], ''{}).

solve([H | X], [H | Y], Env) :-
    char_type(H, digit),
    solve(X, Y, Env).

solve([A | X], [B | Y], Env) :-
    char_type(A, lower),
    char_type(B, digit),
    solve(X, Y, E),
    put_if(A, E, B, Env).

solve([A | X], [B | Y], Env) :-
    char_type(A, digit),
    char_type(B, lower),
    solve(X, Y, E),
    put_if(B, E, A, Env).

solve([A | X], [B | Y], Env) :-
    char_type(A, lower),
    char_type(B, lower),
    solve(X, Y, Env),
    validate_both(A, B, Env).

put_if(Key, Dict, Value, Dict) :-
    get_dict(Key, Dict, Value).

put_if(Key, Dict, Value, Res) :-
    \+ get_dict(Key, Dict, _),
    put_dict(Key, Dict, Value, Res).

validate_both(A, B, Env) :-
    get_dict(A, Env, Val),
    get_dict(B, Env, Val).

validate_both(A, B, Env) :-
    get_dict(A, Env, _),
    \+ get_dict(B, Env, _).

validate_both(A, B, Env) :-
    \+ get_dict(A, Env, _),
    get_dict(B, Env, _).

genC("1").
genC("2").
% genC("3").
genC("a").
genC("b").
% genC("c").

gen(X) :-
    genC(A),
    genC(B),
    genC(C),
    string_concat(A, B, AB),
    string_concat(AB, C, X).

genMain :-
    gen(XS),
    gen(YS),
    solve(XS, YS, Env), 
    writef("%w = %w => %w\n", [XS, YS, Env]),
    false.

main :-
    current_prolog_flag(argv, []),
    genMain.

main :-
    current_prolog_flag(argv, [X, Y]),
    atom_string(X, XS),
    atom_string(Y, YS),
    solve(XS, YS, Env), 
    write_term(Env, [nl(true)]).
