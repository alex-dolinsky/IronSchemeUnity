#!r6rs
(library (macros)
         (export get-prim-typ)
         (import (rnrs (6))
                 (ironscheme clr))

(define-syntax get-prim-typ
    (syntax-rules ()
      [(_ member)
       (clr-field-get PrimitiveType member '())]))

)