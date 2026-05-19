project {
    filter {
    }

    add file{
        folder src{
                a.txt
                b.txt
                c.txt
        }
    }

    relocate *.txt{
        from folder{
            src
        }
        to folder{
            src2
        }
    }
}
