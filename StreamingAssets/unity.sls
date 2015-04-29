#!r6rs
(library (unity)
         (export debug-log
                 new-obj)
         (import (rnrs (6))
                 (ironscheme clr))
         (clr-using UnityEngine)

(define (debug-log obj)
    (clr-static-call Debug Log obj))

(define (new-obj name)
    (clr-new GameObject name))

)