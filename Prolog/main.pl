/* Run with command:   
    swipl -g main -t halt main.pl 12a8 124b
  to solve and find {a:4,b:8}

  Run without arguments to test many permutations
*/

solveS(X, Y, Env) :-
    string_chars(X, XX),
    string_chars(Y, YY),
    solve(XX, YY, Env),
    get_vals(Env, Vals),
    nonvars(Vals).

get_vals(Dict, Vals) :-
    findall(Val, get_dict(_Key, Dict, Val), Vals).

nonvars([]).
nonvars([H | T]) :-
    nonvar(H),
    nonvars(T).

solve([], [], ''{}).

solve([A | X], [B | Y], Env) :-
    solve(X, Y, Env2),
    lookup(A, Env2, C, Env3),
    lookup(B, Env3, C, Env).

lookup(D, Env, D, Env) :-
    char_type(D, digit).

lookup(C, Env, D, Env) :-
    char_type(C, lower),
    get_dict(C, Env, D).

lookup(C, Env, D, Env2) :-
    char_type(C, lower),
    \+ get_dict(C, Env, _),
    put_dict(C, Env, D, Env2).

genC("1").
genC("2").
% genC("3").
genC("a").
genC("b").
% genC("c").

gen(0, "").

gen(N, S) :-
    N > 0,
    M is N - 1,
    gen(M, P),
    genC(C),
    string_concat(P, C, S).

genMain :-
    gen(3, XS),
    gen(3, YS),
    solveS(XS, YS, Env), 
    writef("%w = %w => %w\n", [XS, YS, Env]),
    false.

main :-
    current_prolog_flag(argv, []),
    genMain.

main :-
    current_prolog_flag(argv, [X, Y]),
    atom_string(X, XS),
    atom_string(Y, YS),
    solveS(XS, YS, Env), 
    write_term(Env, [nl(true)]).
