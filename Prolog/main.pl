/* Run with command:   
    swipl -g main -t halt main.pl 12a8 124b
  to solve and find {a:4,b:8}

  Run without arguments to test many permutations
*/

% TODO still mising some cases like solved("C111", "111C", E). doesn't find C=11111 ?
solved(X, Y, Env) :-
    solveS(X, Y, Env1),
    dict_pairs(Env1, _, Pairs),
    flatten_pairs(Pairs, SPairs),
    dict_pairs(Env, '', SPairs).

flatten_pairs([], []).
flatten_pairs([K-Cs | A], [K-S | B]) :-
    [_|_] = Cs,
    forall(member(V, Cs), nonvar(V)),
    string_chars(S, Cs),
    flatten_pairs(A, B).

solveS(X, Y, Env) :-
    string_chars(X, XX),
    string_chars(Y, YY),
    solve(XX, YY, Env).

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

solve([A | X], [B | Y], Env) :-
    char_type(B, upper),
    \+ char_type(A, upper),
    solve([B | Y], [A | X], Env).

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
    append_max(6, D, Ds, Dn), % HACK hardcoded max length of upper
    lookup_many(Cs, Env1, Ds, Env2).

% Hacked versions of append and length that terminate at maximum length.
append_max(_, [], L, L).
append_max(Max, [H | L1], L2, [H | L3]) :-
    length_max(Max, L1, Len1),
    length_max(Max, L2, Len2),
    Len1 + Len2 < Max,
    append_max(Max, L1, L2, L3).

length_max(_, [], 0).
length_max(Max, [_ | T], Len) :-
    length_max(Max, T, SubLen),
    Len is SubLen + 1,
    (  Len =< Max
    -> true
    ;  !, false).

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
    Max is 3,
    findall(S, (between(1, Max, N), gen(N, S)), Out), member(XS, Out),
    findall(S, (between(1, Max, N), gen(N, S)), Out), member(YS, Out),
    solved(XS, YS, Env), 
    writef("%w = %w => %w\n", [XS, YS, Env]),
    false. 

main :-
    current_prolog_flag(argv, []),
    genMain.

main :-
    current_prolog_flag(argv, [X, Y]),
    atom_string(X, XS),
    atom_string(Y, YS),
    solved(XS, YS, Env), 
    write_term(Env, [nl(true)]).
