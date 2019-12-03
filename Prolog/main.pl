/* Run with command:   
    swipl -g main -t halt main.pl 12a8 124b
  to solve and find {a:4,b:8}

  Run without arguments to test many permutations
*/

solveS(X, Y, Env2) :-
    string_chars(X, XX),
    string_chars(Y, YY),
    solve(XX, YY, Env),
    dict_pairs(Env, _, Pairs),
    flatten_pairs(Pairs, SPairs),
    dict_pairs(Env2, '', SPairs).

flatten_pairs([], []).
flatten_pairs([K-Cs | A], [K-S | B]) :-
    forall(member(V, Cs), nonvar(V)),
    string_chars(S, Cs),
    flatten_pairs(A, B).

solve([], [], ''{}).

solve([A | X], [B | Y], Env) :-
    \+ char_type(A, upper),
    \+ char_type(B, upper),
    solve(X, Y, Env2),
    lookup(A, Env2, C, Env3),
    lookup(B, Env3, C, Env),
    C = [_].

solve([A | X], Y, Env) :-
    char_type(A, upper),
    append(Yhead, Ytail, Y),
    solve(X, Ytail, Env1),
    lookup_many(Yhead, Env1, YC, Env2),
    [_|_] = YC,
    lookup(A, Env2, YC, Env).

solve(X, [B | Y], Env) :-
    char_type(B, upper),
    solve([B | Y], X, Env).

lookup(D, Env, [D], Env) :-
    char_type(D, digit).

lookup(C, Env, D, Env) :-
    char_type(C, alpha),
    get_dict(C, Env, D).

lookup(C, Env, [D], Env2) :-
    char_type(C, lower),
    \+ get_dict(C, Env, _),
    put_dict(C, Env, [D], Env2).

lookup(C, Env, D, Env2) :-
    char_type(C, upper),
    \+ get_dict(C, Env, _),
    put_dict(C, Env, D, Env2).

lookup_many([], Env, [], Env).
lookup_many([C | Cs], Env, Dn, Env2) :-
    lookup(C, Env, D, Env1),
    appendShort(D, Ds, Dn), % TODO why does this not work, and why does lookup_many(['A'], r{}, Ds, E). only give two ans?
    lookup_many(Cs, Env1, Ds, Env2).

appendShort([], L, L).
appendShort([H | L1], L2, [H | L3]) :-
    appendShort(L1, L2, L3),
    length(L3, Len),
    (  Len < 50
    -> 1 = 1
    ;  !).

genC("1").
genC("2").
genC("3").
genC("a").
genC("b").
genC("C").
genC("D").

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
